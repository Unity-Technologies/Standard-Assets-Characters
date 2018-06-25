using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public abstract class InputThirdPersonMotor : BaseThirdPersonMotor
	{
		/// <summary>
		/// Movement values
		/// </summary>
		[SerializeField]
		protected Transform cameraTransform;

		[SerializeField]
		protected AnimationCurve jumpBraceAsAFunctionOfSpeed;

		[SerializeField]
		protected float jumpCooldown = 0.5f;

		[SerializeField]
		protected float jumpSpeed = 15f;
		
		[SerializeField, Range(0f, 1f)]
		protected float airborneTurnSpeedProportion = 0.5f;

		[SerializeField]
		protected float angleSnapBehaviour = 120f;

		[SerializeField]
		protected float maxTurnSpeed = 10000f;

		[SerializeField]
		protected AnimationCurve turnSpeedAsAFunctionOfForwardSpeed = AnimationCurve.Linear(0, 0, 1, 1);

		[SerializeField]
		protected float snappingDecelaration = 30;

		[SerializeField, Range(0, 1), Tooltip("Fraction of max forward speed")]
		protected float snapSpeedTarget = 0.2f;

		[SerializeField]
		protected InputResponse runInput;

		[SerializeField]
		protected InputResponse strafeInput;

		/// <summary>
		/// The input implementation
		/// </summary>
		protected ICharacterInput characterInput;

		/// <summary>
		/// The physic implementation
		/// </summary>
		protected ICharacterPhysics characterPhysics;

		protected bool isRunToggled, isStrafing;

		protected DateTime nextAllowedJumpTime;

		protected bool isBracingForJump;
		protected float jumpBraceTime, jumpBraceCount;


		public override float fallTime
		{
			get { return characterPhysics.fallTime; }
		}
		
		private float normalizedSpeed
		{
			get
			{
				return  Mathf.Max(Mathf.Abs(normalizedForwardSpeed), Mathf.Abs(normalizedLateralSpeed));
			}
		}

		/// <inheritdoc />
		/// <summary>
		/// Gets required components
		/// </summary>
		protected override void Awake()
		{
			characterInput = GetComponent<ICharacterInput>();
			characterPhysics = GetComponent<ICharacterPhysics>();

			if (runInput != null)
			{
				runInput.Init();
			}

			if (strafeInput != null)
			{
				strafeInput.Init();
			}

			base.Awake();
		}

		private void OnRunEnded()
		{
			isRunToggled = false;
		}

		private void OnRunStarted()
		{
			isRunToggled = true;
		}

		private void OnStrafeEnd()
		{
			isStrafing = false;
		}

		private void OnStrafeStart()
		{
			isStrafing = true;
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		protected virtual void OnEnable()
		{
			characterInput.jumpPressed += OnJumpPressed;
			characterPhysics.landed += OnLanding;
			characterPhysics.startedFalling += OnStartedFalling;

			if (runInput != null)
			{
				runInput.started += OnRunStarted;
				runInput.ended += OnRunEnded;
			}

			if (strafeInput != null)
			{
				strafeInput.started += OnStrafeStart;
				strafeInput.ended += OnStrafeEnd;
			}
		}

		/// <summary>
		/// Handles player fall start
		/// </summary>
		private void OnStartedFalling(float predictedFallDistance)
		{
			if (fallStarted != null)
			{
				fallStarted(predictedFallDistance);
			}
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		protected virtual void OnDisable()
		{
			if (characterInput != null)
			{
				characterInput.jumpPressed -= OnJumpPressed;
			}

			if (characterPhysics != null)
			{
				characterPhysics.landed -= OnLanding;
				characterPhysics.startedFalling -= OnStartedFalling;
			}

			if (runInput != null)
			{
				runInput.started -= OnRunStarted;
				runInput.ended -= OnRunEnded;
			}

			if (strafeInput != null)
			{
				strafeInput.started -= OnStrafeStart;
				strafeInput.ended -= OnStrafeEnd;
			}
		}

		/// <summary>
		/// Handles player landing
		/// </summary>
		private void OnLanding()
		{
			if (landed != null)
			{
				landed();
			}
			nextAllowedJumpTime = DateTime.Now.AddSeconds(jumpCooldown);
		}

		/// <summary>
		/// Subscribes to the Jump action on input
		/// </summary>
		private void OnJumpPressed()
		{
			if (!characterPhysics.isGrounded || DateTime.Now < nextAllowedJumpTime)
			{
				return;
			}

			if (jumpStarted != null)
			{
				jumpStarted();
			}

			jumpBraceTime = jumpBraceAsAFunctionOfSpeed.Evaluate(normalizedSpeed);
			isBracingForJump = true;
		}

		/// <summary>
		/// Called when the brace to jump completes
		/// </summary>
		private void OnJumpBraceComplete()
		{
			characterPhysics.SetJumpVelocity(jumpSpeed);
			isBracingForJump = false;
			jumpBraceCount = 0;
		}

		/// <summary>
		/// Handles the timing of the jump brace
		/// </summary>
		private void Update()
		{
			if (isBracingForJump)
			{
				jumpBraceCount += Time.deltaTime;
				if (jumpBraceCount >= jumpBraceTime)
				{
					OnJumpBraceComplete();
				}
			}
		}

		/// <summary>
		/// Movement Logic on physics update
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (isStrafing)
			{
				SetStrafeLookDirection();
				CalculateStrafeMovement();
			}
			else
			{
				SetForwardLookDirection();
				CalculateForwardMovement();
			}

			CalculateYRotationSpeed(Time.fixedDeltaTime);
		}

		protected abstract void CalculateForwardMovement();

		protected abstract void CalculateStrafeMovement();

		/// <summary>
		/// Sets forward rotation
		/// </summary>
		private void SetForwardLookDirection()
		{
			if (!CanSetForwardLookDirection())
			{
				return;
			}

			Vector3 flatForward = cameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection =
				new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);

			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			Quaternion targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);	
			HandleTargetRotation(targetRotation);
		}

		protected abstract bool CanSetForwardLookDirection();
		
		protected abstract void HandleTargetRotation(Quaternion targetRotation);

		/// <summary>
		/// Sets forward rotation
		/// </summary>
		private void SetStrafeLookDirection()
		{
			Vector3 lookForwardY = cameraTransform.rotation.eulerAngles;
			lookForwardY.x = 0;
			lookForwardY.z = 0;
			Quaternion targetRotation = Quaternion.Euler(lookForwardY);

			float actualTurnSpeed =
				characterPhysics.isGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
			targetRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

			transform.rotation = targetRotation;
		}
	}
}
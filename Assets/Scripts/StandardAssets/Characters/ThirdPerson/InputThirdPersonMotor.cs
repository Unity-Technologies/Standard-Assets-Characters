using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Events;

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
		protected float rapidTurnAngle = 180f;
		
		[SerializeField]
		protected float rapidTurnForwardSpeedThreshold = 0.1f;

		[SerializeField]
		protected float postRapidTurnRotationEasingSpeed = 1f, postRapidTurnEasingTime = 1f; 

		[SerializeField]
		protected InputResponse strafeInput;

		[SerializeField]
		protected UnityEvent startActionMode, startStrafeMode;

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

		protected float currentEasingTime;

		protected RapidTurningState rapidTurningState = RapidTurningState.None;

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
		
		public override void FinishedTurn()
		{
			ResetRotation();
			currentEasingTime = 0;
			rapidTurningState = RapidTurningState.Easing;
		}

		protected virtual void ResetRotation()
		{
			transform.rotation = CalculateTargetRotation();
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
			
			OnStrafeEnd();

			base.Awake();
		}

		protected virtual  void OnStrafeEnd()
		{
			if (startActionMode != null)
			{
				startActionMode.Invoke();
			}
			isStrafing = false;
		}

		protected virtual void OnStrafeStart()
		{
			if (startStrafeMode != null)
			{
				startStrafeMode.Invoke();
			}
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
		
		protected virtual void OnRunEnded()
		{
			isRunToggled = false;
		}

		protected virtual void OnRunStarted()
		{
			isRunToggled = true;
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
			if (!characterPhysics.isGrounded)
			{
				return;
			}
			
			if (rapidTurningState == RapidTurningState.Easing)
			{
				RapidTurningEasing();
				return;
			}
			
			if (rapidTurningState == RapidTurningState.Turning)
			{
				return;
			}

			if (!CanSetForwardLookDirection())
			{
				return;
			}

			Quaternion targetRotation = CalculateTargetRotation();
			
			float angleDifference = Mathf.Abs((transform.eulerAngles - targetRotation.eulerAngles).y);

			if (normalizedForwardSpeed > rapidTurnForwardSpeedThreshold && rapidTurnAngle < angleDifference && angleDifference < 360 - rapidTurnAngle)
			{
				rapidlyTurned(0.2f);
				rapidTurningState = RapidTurningState.Turning;
				return;
			}

			HandleTargetRotation(targetRotation);
		}

		private void RapidTurningEasing()
		{
			if (!characterInput.hasMovementInput)
			{
				return;
			}
			
			Quaternion targetRotation = CalculateTargetRotation();
			
			transform.rotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, postRapidTurnRotationEasingSpeed * Time.fixedDeltaTime);

			currentEasingTime += Time.fixedDeltaTime;

			if (currentEasingTime >= postRapidTurnEasingTime)
			{
				rapidTurningState = RapidTurningState.None;
				currentEasingTime = 0;
			}
		}

		private Quaternion CalculateTargetRotation()
		{
			Vector3 flatForward = cameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection =
				new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);

			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		protected abstract bool CanSetForwardLookDirection();
		
		protected abstract void HandleTargetRotation(Quaternion targetRotation);

		/// <summary>
		/// Sets forward rotation
		/// </summary>
		private void SetStrafeLookDirection()
		{
			if (!characterPhysics.isGrounded)
			{
				return;
			}
			
			Vector3 lookForwardY = transform.rotation.eulerAngles;
			
			lookForwardY.x = 0;
			lookForwardY.z = 0;
			//TODO: DAVE
			//lookForwardY.y = lookForwardY.y + characterInput.lookInput.x * Time.fixedDeltaTime;
			lookForwardY.y -= characterInput.lookInput.x;
			
			Quaternion targetRotation = Quaternion.Euler(lookForwardY);

			float actualTurnSpeed =
				characterPhysics.isGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
			targetRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

			transform.rotation = targetRotation;
			
		
		}
	}
}
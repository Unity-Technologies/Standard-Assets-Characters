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
		protected enum State
		{
			Moving,
			RapidTurnDecel,
			RapidTurnRotation
		}

		/// <summary>
		/// Movement values
		/// </summary>
		[SerializeField]
		private Transform cameraTransform;

		[SerializeField]
		private AnimationCurve jumpBraceAsAFunctionOfSpeed;

		[SerializeField]
		private float jumpCooldown = 2;

		[SerializeField]
		private float jumpSpeed = 15f;
		
		[SerializeField, Range(0f, 1f)]
		private float airborneTurnSpeedProportion = 0.5f;

		[SerializeField]
		private float angleSnapBehaviour = 120f;

		[SerializeField]
		private float maxTurnSpeed = 10000f;

		[SerializeField]
		private AnimationCurve turnSpeedAsAFunctionOfForwardSpeed = AnimationCurve.Linear(0, 0, 1, 1);

		[SerializeField]
		protected float snappingDecelaration = 30;

		[SerializeField, Range(0, 1), Tooltip("Fraction of max forward speed")]
		protected float snapSpeedTarget = 0.2f;

		[SerializeField]
		private InputResponse runInput;

		[SerializeField]
		private InputResponse strafeInput;

		/// <summary>
		/// The input implementation
		/// </summary>
		protected ICharacterInput characterInput;

		/// <summary>
		/// The physic implementation
		/// </summary>
		protected ICharacterPhysics characterPhysics;

		protected bool isRunToggled, isStrafing;

		private float currentForwardSpeed;

		private float currentLateralSpeed;

		protected State state;

		private DateTime nextAllowedJumpTime;

		private bool isBracingForJump;
		private float jumpBraceTime, jumpBraceCount;


		public override float fallTime
		{
			get { return characterPhysics.fallTime; }
		}

		/// <inheritdoc />
		/// <summary>
		/// Gets required components
		/// </summary>
		protected override void Awake()
		{
			characterInput = GetComponent<ICharacterInput>();
			characterPhysics = GetComponent<ICharacterPhysics>();

			runInput.Init();
			strafeInput.Init();

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
		private void OnEnable()
		{
			characterInput.jumpPressed += OnJumpPressed;
			characterPhysics.landed += OnLanding;
			characterPhysics.startedFalling += OnStartedFalling;

			runInput.started += OnRunStarted;
			runInput.ended += OnRunEnded;

			if (strafeInput != null)
			{
				strafeInput.started += OnStrafeStart;
				strafeInput.ended += OnStrafeEnd;
			}
		}

		/// <summary>
		/// Handles player fall start
		/// </summary>
		private void OnStartedFalling()
		{
			if (fallStarted != null)
			{
				fallStarted();
			}
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		private void OnDisable()
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

			jumpBraceTime = jumpBraceAsAFunctionOfSpeed.Evaluate(normalizedForwardSpeed);
			nextAllowedJumpTime = DateTime.Now.AddSeconds(jumpCooldown);
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
			// if no input or decelerating for a rapid turn, we early out
			if (!characterInput.hasMovementInput || state == State.RapidTurnDecel)
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

			float angleDifference = Mathf.Abs((transform.eulerAngles - targetRotation.eulerAngles).y);

			float calculatedTurnSpeed = turnSpeed;
			if (angleSnapBehaviour < angleDifference && angleDifference < 360 - angleSnapBehaviour)
			{
				// if we need a rapid turn, first decelerate
				if (state == State.Moving)
				{
					state = State.RapidTurnDecel;
					return;
				}

				// rapid turn deceleration complete, now we rotate appropriately.
				calculatedTurnSpeed += (maxTurnSpeed - turnSpeed) *
				                       turnSpeedAsAFunctionOfForwardSpeed.Evaluate(Mathf.Abs(normalizedForwardSpeed));
			}
			// once rapid turn rotation is complete, we return to the normal movement state
			else if (state == State.RapidTurnRotation)
			{
				state = State.Moving;
			}

			float actualTurnSpeed = calculatedTurnSpeed;
			if (!characterPhysics.isGrounded)
			{
				actualTurnSpeed *= airborneTurnSpeedProportion;
			}

			targetRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

			transform.rotation = targetRotation;
		}

		/// <summary>
		/// Sets forward rotation
		/// </summary>
		private void SetStrafeLookDirection()
		{
			Vector3 lookForwardY = cameraTransform.rotation.eulerAngles;
			lookForwardY.x = 0;
			lookForwardY.z = 0;
			Quaternion targetRotation = Quaternion.Euler(lookForwardY);

			float angleDifference = Mathf.Abs((transform.eulerAngles - targetRotation.eulerAngles).y);

			float actualTurnSpeed =
				characterPhysics.isGrounded ? turnSpeed : turnSpeed * airborneTurnSpeedProportion;
			targetRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

			transform.rotation = targetRotation;
		}
	}
}
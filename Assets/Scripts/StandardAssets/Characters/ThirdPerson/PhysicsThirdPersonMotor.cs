using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// The main third person controller
	/// </summary>
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public class PhysicsThirdPersonMotor : BaseThirdPersonMotor
	{
		private enum State
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
		private float maxForwardSpeed = 10f, maxLateralSpeed = 10f;
		
		[SerializeField]
		private bool useAcceleration = true;
		
		[SerializeField]    
		private float groundAcceleration = 20f;
		
		[SerializeField]
		private float groundDeceleration = 15f;

		[SerializeField, Range(0f, 1f)]
		private float airborneAccelProportion = 0.5f;

		[SerializeField, Range(0f, 1f)]
		private float airborneDecelProportion = 0.5f;

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
		private float snappingDecelaration = 30;
		
		[SerializeField, Range(0,1), Tooltip("Fraction of max forward speed")]
		private float snapSpeedTarget = 0.2f;

		[SerializeField] 
		private PhysicsMotorProperties physicsMotorProperties;

		[SerializeField] 
		private InputResponse runInput;
		
		[SerializeField] 
		private InputResponse strafeInput;

		/// <summary>
		/// The input implementation
		/// </summary>
		private ICharacterInput characterInput;

		/// <summary>
		/// The physic implementation
		/// </summary>
		private ICharacterPhysics characterPhysics;

		private bool isRunToggled, isStrafing;

		private float currentForwardSpeed;
		
		private float currentLateralSpeed;

		private State state;
		
		private DateTime nextAllowedJumpTime;
		
		private bool isBracingForJump;
		private float jumpBraceTime, jumpBraceCount;
		
		/// <inheritdoc />
		public override float normalizedLateralSpeed
		{
			get { return -currentLateralSpeed / maxLateralSpeed; }
		}

		/// <inheritdoc />
		public override float normalizedForwardSpeed
		{
			get { return currentForwardSpeed / maxForwardSpeed; }
		}

		public override float fallTime
		{
			get { return characterPhysics.fallTime; }
		}

		private float currentMaxForwardSpeed
		{
			get { return maxForwardSpeed * (isRunToggled ? physicsMotorProperties.runSpeedProportion : 
							physicsMotorProperties.walkSpeedProporiton); }
		}
		
		private float currentGroundAccelaration
		{
			get { return groundAcceleration * (isRunToggled ? physicsMotorProperties.runAccelerationProportion : 
							physicsMotorProperties.walkAccelerationProporiton); }
		}
		
		public float currentSpeedForUi
		{
			get { return maxForwardSpeed; }
			set { maxForwardSpeed = value; }
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
		/// Caleld when the brace to jump completes
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
		private void FixedUpdate()
		{
			if (isStrafing)
			{
				SetLookDirection();
				CalculateMovement();
			}
			else
			{
				SetForward();
				CalculateForwardMovement();
			}
			if (animator == null)
			{
				Move();
			}
			CalculateYRotationSpeed(Time.fixedDeltaTime);
		}

		/// <summary>
		/// Handle movement if the animator is set
		/// </summary>
		private void OnAnimatorMove()
		{
			if (animator != null)
			{
				Move();
			}
		}

		/// <summary>
		/// Sets forward rotation
		/// </summary>
		private void SetForward()
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
		/// Calculates the forward movement
		/// </summary>
		private void CalculateForwardMovement()
		{
			Vector2 moveInput = characterInput.moveInput;
			if (moveInput.sqrMagnitude > 1f)
			{
				moveInput.Normalize();
			}

			float desiredSpeed = moveInput.magnitude * currentMaxForwardSpeed;
			if (useAcceleration)
			{
				float acceleration = characterInput.hasMovementInput ? currentGroundAccelaration : groundDeceleration;
				if (!characterPhysics.isGrounded)
				{
					acceleration *= airborneDecelProportion;
				}

				if (state == State.RapidTurnDecel) // rapid turn
				{
					var target = snapSpeedTarget * currentMaxForwardSpeed;
					currentForwardSpeed =
						Mathf.MoveTowards(currentForwardSpeed, target, snappingDecelaration * Time.fixedDeltaTime);
					if (currentForwardSpeed <= target)
					{
						state = State.RapidTurnRotation;
					}
				}
				else
				{
					currentForwardSpeed =
						Mathf.MoveTowards(currentForwardSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
				}
			}
			else
			{
				currentForwardSpeed = desiredSpeed;
			}
		}
		
		/// <summary>
		/// Sets forward rotation
		/// </summary>
		private void SetLookDirection()
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

		/// <summary>
		/// Calculates the forward movement
		/// </summary>
		private void CalculateMovement()
		{
			Vector2 moveInput = characterInput.moveInput;

			currentForwardSpeed = CalculateSpeed(moveInput.y * maxForwardSpeed, currentForwardSpeed);
			currentLateralSpeed = CalculateSpeed(moveInput.x * maxLateralSpeed, currentLateralSpeed);
		}
		
		private float CalculateSpeed(float desiredSpeed, float currentSpeed)
		{
			if (useAcceleration)
			{
				float acceleration = characterPhysics.isGrounded
					? (characterInput.hasMovementInput ? groundAcceleration : groundDeceleration)
					: (characterInput.hasMovementInput ? groundAcceleration : groundDeceleration) *
					  airborneDecelProportion;

				return 
					Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
			}
			else
			{
				return desiredSpeed;
			}
		}

		/// <summary>
		/// Moves the character
		/// </summary>
		private void Move()
		{
			Vector3 movement;

			if (isStrafing)
			{
				if (animator != null && characterPhysics.isGrounded &&
				    animator.deltaPosition.z >= groundAcceleration * Time.fixedDeltaTime)
				{
					movement = animator.deltaPosition;
				}
				else
				{
					Vector3 lateral = currentLateralSpeed * transform.right * Time.fixedDeltaTime;
					Vector3 forward = currentForwardSpeed * transform.forward * Time.fixedDeltaTime;
      
					movement = forward + lateral;
				}
			}
			else
			{
				if (animator != null && characterPhysics.isGrounded &&
				    animator.deltaPosition.z >= currentGroundAccelaration * Time.deltaTime)
				{
					movement = animator.deltaPosition;
				}
				else
				{
					movement = currentForwardSpeed * transform.forward * Time.fixedDeltaTime;
				}
			}
			
			characterPhysics.Move(movement);
		}
	}
}
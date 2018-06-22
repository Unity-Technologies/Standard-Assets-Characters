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
	public class PhysicsThirdPersonMotor : InputThirdPersonMotor
	{
		protected enum State
		{
			Moving,
			RapidTurnDecel,
			RapidTurnRotation
		}
		
		[SerializeField]
		private float maxForwardSpeed = 10f, maxLateralSpeed = 10f;
		
		[SerializeField]
		private bool useAcceleration = true;
		
		[SerializeField]    
		private float groundAcceleration = 20f;
		
		[SerializeField]
		private float groundDeceleration = 15f;

		[SerializeField, Range(0f, 1f)]
		private float airborneDecelProportion = 0.5f;
		
		
		protected State state;
		
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

		private float currentMaxLateralSpeed
		{
			get { return maxLateralSpeed * (isRunToggled ? physicsMotorProperties.runSpeedProportion : 
				             physicsMotorProperties.walkSpeedProporiton); }
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


		/// <summary>
		/// Movement Logic on physics update
		/// </summary>
		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			if (animator == null)
			{
				Move();
			}
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
		/// Calculates the forward movement
		/// </summary>
		protected override void CalculateForwardMovement()
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
		/// Calculates the forward movement
		/// </summary>
		protected override void CalculateStrafeMovement()
		{
			Vector2 moveInput = characterInput.moveInput;

			currentForwardSpeed = CalculateSpeed(moveInput.y * currentMaxForwardSpeed, currentForwardSpeed);
			currentLateralSpeed = CalculateSpeed(moveInput.x * currentMaxLateralSpeed, currentLateralSpeed);
		}

		protected override bool CanSetForwardLookDirection()
		{
			// if no input or decelerating for a rapid turn, we early out
			return characterInput.hasMovementInput && state != State.RapidTurnDecel;
		}

		protected override void HandleTargetRotation(Quaternion targetRotation)
		{
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
		
		private float CalculateSpeed(float desiredSpeed, float currentSpeed)
		{
			if (!useAcceleration)
			{
				return desiredSpeed;
			}

			float acceleration = characterInput.hasMovementInput ? currentGroundAccelaration : groundDeceleration;
			if (!characterPhysics.isGrounded)
			{
				acceleration *= airborneDecelProportion;
			}
			return Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
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
				    animator.deltaPosition.z >= currentGroundAccelaration * Time.fixedDeltaTime)
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
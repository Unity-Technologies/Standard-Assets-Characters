using StandardAssets.Characters.Physics;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	public class AnimationThirdPersonMotor : InputThirdPersonMotor
	{
		[SerializeField]
		protected AnimationMotorProperties animationMotorProperties;

		[SerializeField]
		protected bool inheritGroundVelocity;

		[SerializeField, Tooltip("A fall distance higher than this will trigger a fall animation")]
		protected float maxFallDistanceToLand = 1;

		private float normalizedInputLateralSpeed;
		private float normalizedInputForwardSpeed;
		private float forwardClampSpeed, targetForwardClampSpeed, lateralClampSpeed, targetLateralClampSpeed;

		private AnimationInputProperties currentForwardInputProperties, currentLateralInputProperties;

		private Vector3 groundMovementVector, cacheGroundMovementVector;

		public override float normalizedLateralSpeed
		{
			get { return -Mathf.Clamp(normalizedInputLateralSpeed, -1, 1); }
		}

		public override float normalizedForwardSpeed
		{
			get { return Mathf.Clamp(normalizedInputForwardSpeed, -1, 1); }
		}

		public void OnJumpAnimationComplete()
		{
			var baseCharacterPhysics = GetComponent<BaseCharacterPhysics>();
			if (baseCharacterPhysics == null)
			{
				return;
			}

			var distance = baseCharacterPhysics.GetPredicitedFallDistance();
			if (distance <= maxFallDistanceToLand)
			{
				if (landed != null)
				{
					landed();
				}
			}
			else
			{
				if (fallStarted != null)
				{
					fallStarted(distance);
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			currentForwardInputProperties = animationMotorProperties.forwardMovementProperties;
			targetForwardClampSpeed = forwardClampSpeed = currentForwardInputProperties.inputClamped;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (inheritGroundVelocity)
			{
				characterInput.jumpPressed += CacheCurrentMovement;
			}
		}

		protected override void OnRunEnded()
		{
			base.OnRunEnded();
			targetForwardClampSpeed = currentForwardInputProperties.inputClamped;
			if (isStrafing)
			{
				targetLateralClampSpeed = currentLateralInputProperties.inputClamped;
			}
		}

		protected override void OnRunStarted()
		{
			base.OnRunStarted();
			targetForwardClampSpeed = currentForwardInputProperties.inputUnclamped;
			if (isStrafing)
			{
				targetLateralClampSpeed = currentLateralInputProperties.inputUnclamped;
			}
		}

		protected override void OnStrafeStart()
		{
			base.OnStrafeStart();
			rapidTurningState = RapidTurningState.None;
			currentForwardInputProperties = animationMotorProperties.strafeForwardMovementProperties;
			currentLateralInputProperties = animationMotorProperties.strafeLateralMovementProperties;
		}

		protected override void OnStrafeEnd()
		{
			base.OnStrafeEnd();
			currentForwardInputProperties = animationMotorProperties.forwardMovementProperties;
			currentLateralInputProperties = null;
		}

		protected override void ResetRotation()
		{
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, animator.bodyRotation.eulerAngles.y,
			                                    transform.eulerAngles.z);
		}

		private void CacheCurrentMovement()
		{
			cacheGroundMovementVector = groundMovementVector;
		}

		protected override void CalculateForwardMovement(float deltaTime)
		{
			normalizedInputLateralSpeed = 0;

			if (!characterPhysics.isGrounded)
			{
				return;
			}

			if (!characterInput.hasMovementInput)
			{
				EaseOffForwardInput(deltaTime);
				return;
			}

			ApplyForwardInput(1f, deltaTime);
		}

		protected override void CalculateStrafeMovement(float deltaTime)
		{
			if (!characterPhysics.isGrounded)
			{
				return;
			}

			Vector2 moveInput = characterInput.moveInput;

			// we need to ease each axis
			if (Mathf.Abs(moveInput.y) > Mathf.Epsilon)
			{
				ApplyForwardInput(Mathf.Sign(moveInput.y), deltaTime);
			}
			else
			{
				EaseOffForwardInput(deltaTime);
			}

			if (Mathf.Abs(moveInput.x) > Mathf.Epsilon)
			{
				ApplyLateralInput(Mathf.Sign(moveInput.x), deltaTime);
			}
			else
			{
				EaseOffLateralInput(deltaTime);
			}
		}

		protected override bool CanSetForwardLookDirection()
		{
			return characterInput.hasMovementInput;
		}

		protected override void HandleTargetRotation(Quaternion targetRotation, float deltaTime)
		{
			float angleDifference = Mathf.Abs((transform.eulerAngles - targetRotation.eulerAngles).y);

			float calculatedTurnSpeed = turnSpeed;
			if (angleSnapBehaviour < angleDifference && angleDifference < 360 - angleSnapBehaviour)
			{
				// rapid turn deceleration complete, now we rotate appropriately.
				calculatedTurnSpeed += (maxTurnSpeed - turnSpeed) *
				                       turnSpeedAsAFunctionOfForwardSpeed.Evaluate(Mathf.Abs(normalizedForwardSpeed));
			}

			float actualTurnSpeed = calculatedTurnSpeed;
			if (!characterPhysics.isGrounded)
			{
				actualTurnSpeed *= airborneTurnSpeedProportion;
			}

			targetRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * deltaTime);

			transform.rotation = targetRotation;
		}

		private void ApplyForwardInput(float input, float deltaTime)
		{
			if (rapidTurningState != RapidTurningState.None)
			{
				return;
			}

			float forwardVelocity = currentForwardInputProperties.inputGain;
			if (Mathf.Abs(Mathf.Sign(input) - Mathf.Sign(normalizedInputForwardSpeed)) > 0)
			{
				forwardVelocity = currentForwardInputProperties.inputChangeGain;
			}

			normalizedInputForwardSpeed =
				Mathf.Clamp(normalizedInputForwardSpeed + input * forwardVelocity * deltaTime, -forwardClampSpeed,
				            forwardClampSpeed);
		}

		private void EaseOffForwardInput(float deltaTime)
		{
			normalizedInputForwardSpeed =
				Mathf.Lerp(normalizedInputForwardSpeed, 0, currentForwardInputProperties.inputDecay * deltaTime);
		}

		private void ApplyLateralInput(float input, float deltaTime)
		{
			float lateralVelocity = currentLateralInputProperties.inputGain;
			if (Mathf.Abs(Mathf.Sign(input) - Mathf.Sign(normalizedInputLateralSpeed)) > 0)
			{
				lateralVelocity = currentLateralInputProperties.inputChangeGain;
			}

			normalizedInputLateralSpeed =
				Mathf.Clamp(normalizedInputLateralSpeed + input * lateralVelocity * deltaTime, -forwardClampSpeed,
				            forwardClampSpeed);
		}

		private void EaseOffLateralInput(float deltaTime)
		{
			normalizedInputLateralSpeed =
				Mathf.Lerp(normalizedInputLateralSpeed, 0, currentLateralInputProperties.inputDecay * deltaTime);
		}

		private void OnAnimatorMove()
		{
			if (!inheritGroundVelocity || characterPhysics.isGrounded)
			{
				groundMovementVector = new Vector3(animator.deltaPosition.x, 0, animator.deltaPosition.z);
				characterPhysics.Move(groundMovementVector);
			}
			else
			{
				characterPhysics.Move(cacheGroundMovementVector);
			}
		}

		private float DecelerateClampSpeed(float currentValue, float targetValue, float gain)
		{
			if (currentValue <= targetValue)
			{
				return targetValue;
			}

			return Mathf.Lerp(currentValue, targetValue, Time.deltaTime * gain);
		}

		private void HandleClampSpeedDeceleration()
		{
			forwardClampSpeed = DecelerateClampSpeed(forwardClampSpeed, targetForwardClampSpeed,
			                                         currentForwardInputProperties.inputDecay);

			if (isStrafing)
			{
				lateralClampSpeed = DecelerateClampSpeed(lateralClampSpeed, targetLateralClampSpeed,
				                                         currentLateralInputProperties.inputDecay);
			}
		}

		protected override void Update()
		{
			base.Update();
			HandleMovementLogic(Time.deltaTime);
			HandleClampSpeedDeceleration();
		}
	}
}
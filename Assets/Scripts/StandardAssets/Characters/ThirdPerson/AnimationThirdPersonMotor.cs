using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	public class AnimationThirdPersonMotor : InputThirdPersonMotor
	{
		[SerializeField]
		private CurveEvaluator forwardInputIncrement,
		                       forwardInputDecrement,
		                       lateralInputIncrement,
		                       lateralInputDecrement;

		[SerializeField]
		private bool inheritGroundVelocity;

		private float normalizedInputLateralSpeed;
		private float normalizedInputForwardSpeed;

		private float forwardInputIncrementTime, forwardInputDecrementTime, lateralInputIncrementTime, lateralInputDecrementTime;

		private Vector3 groundMovementVector, cacheGroundMovementVector;

		public override float normalizedLateralSpeed
		{
			get { return Mathf.Clamp(normalizedInputLateralSpeed, -1, 1); }
		}

		public override float normalizedForwardSpeed
		{
			get { return Mathf.Clamp(normalizedInputForwardSpeed, -1, 1); }
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (inheritGroundVelocity)
			{
				characterInput.jumpPressed += CacheCurrentMovement;
			}
		}

		private void CacheCurrentMovement()
		{
			cacheGroundMovementVector = groundMovementVector;
		}

		protected override void CalculateForwardMovement()
		{
			if (!characterInput.hasMovementInput)
			{
				EaseOffForwardInput();
				characterPhysics.Move(Vector3.zero);
				return;
			}

			ApplyForwardInput();
		}

		protected override void CalculateStrafeMovement()
		{
			Vector2 moveInput = characterInput.moveInput;

			// we need to ease each axis
			if (Mathf.Abs(moveInput.y) > Mathf.Epsilon)
			{
				ApplyForwardInput();
			}
			else
			{
				EaseOffForwardInput();
			}
			
			if (Mathf.Abs(moveInput.x) > Mathf.Epsilon)
			{
				ApplyLateralInput();
			}
			else
			{
				EaseOffLateralInput();
			}
			
			normalizedInputForwardSpeed = normalizedForwardSpeed * Mathf.Sign(moveInput.y);
			normalizedInputLateralSpeed = normalizedLateralSpeed * Mathf.Sign(moveInput.x);
		}

		protected override bool CanSetForwardLookDirection()
		{
			return characterInput.hasMovementInput;
		}

		protected override void HandleTargetRotation(Quaternion targetRotation)
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
				Quaternion.RotateTowards(transform.rotation, targetRotation, actualTurnSpeed * Time.fixedDeltaTime);

			transform.rotation = targetRotation;
		}

		private float  InputCurveEvaluate(CurveEvaluator curve, ref float zeroTime,
		                                ref float incrementingTime)
		{
			zeroTime = 0;
			incrementingTime += Time.fixedDeltaTime;
			return curve.Evaluate(incrementingTime);
		}
		

		private void ApplyForwardInput()
		{
			normalizedInputForwardSpeed = 
				InputCurveEvaluate(forwardInputIncrement, ref forwardInputDecrementTime, ref forwardInputIncrementTime);
		}

		private void EaseOffForwardInput()
		{
			normalizedInputForwardSpeed =
				InputCurveEvaluate(forwardInputDecrement, ref forwardInputIncrementTime, ref forwardInputDecrementTime);
		}

		private void ApplyLateralInput()
		{
			normalizedInputLateralSpeed = 
				InputCurveEvaluate(lateralInputIncrement, ref lateralInputDecrementTime, ref lateralInputIncrementTime);
		}
		
		private void EaseOffLateralInput()
		{
			normalizedInputLateralSpeed =
				InputCurveEvaluate(lateralInputDecrement, ref lateralInputIncrementTime, ref lateralInputDecrementTime);
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
	}
}
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class AnimationThirdPersonMotor : InputThirdPersonMotor
	{
		[SerializeField]
		private float inputIncreaseTime = 2f, inputDecreaseTime = 0.5f;

		[SerializeField]
		private bool inheritGroundVelocity;

		private float normalizedInputLateralSpeed;
		private float normalizedInputForwardSpeed;

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
				EaseOffInput();
				characterPhysics.Move(Vector3.zero);
				return;
			}

			normalizedInputForwardSpeed += Time.deltaTime / inputIncreaseTime;
		}

		protected override void CalculateStrafeMovement()
		{
			if (!characterInput.hasMovementInput)
			{
				EaseOffInput();
				characterPhysics.Move(Vector3.zero);
				return;
			}

			Vector2 moveInput = characterInput.moveInput;

			normalizedInputLateralSpeed += Mathf.Sign(moveInput.x) * Time.fixedDeltaTime / inputIncreaseTime;
			normalizedInputForwardSpeed += Mathf.Sign(moveInput.y) * Time.fixedDeltaTime / inputIncreaseTime;
		}

		private void EaseOffInput()
		{
			normalizedInputForwardSpeed =
				Mathf.Lerp(normalizedInputForwardSpeed, 0, Time.fixedDeltaTime / inputDecreaseTime);
			normalizedInputLateralSpeed =
				Mathf.Lerp(normalizedInputLateralSpeed, 0, Time.fixedDeltaTime / inputDecreaseTime);
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
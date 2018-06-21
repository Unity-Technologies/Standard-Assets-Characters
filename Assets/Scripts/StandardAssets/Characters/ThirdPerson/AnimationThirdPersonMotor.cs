using System.Runtime.Serialization.Formatters;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class AnimationThirdPersonMotor : InputThirdPersonMotor
	{
		[SerializeField]
		private float inputIncreaseTime = 2f, inputDecreaseTime = 0.5f;
		
		private float normalizedInputLateralSpeed;
		private float normalizedInputForwardSpeed;

		public override float normalizedLateralSpeed
		{
			get { return Mathf.Clamp(normalizedInputLateralSpeed, -1, 1); }
		}

		public override float normalizedForwardSpeed
		{
			get { return Mathf.Clamp(normalizedInputForwardSpeed, -1, 1); }
		}

		protected override void CalculateForwardMovement()
		{
			if (!characterInput.hasMovementInput)
			{
				EaseOffInput();
				characterPhysics.Move(Vector3.zero);
				return;
			}

			normalizedInputForwardSpeed += Time.deltaTime/inputIncreaseTime;
			characterPhysics.Move(Vector3.zero);

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
			
			normalizedInputLateralSpeed += Mathf.Sign(moveInput.x) * Time.fixedDeltaTime/inputIncreaseTime;
			normalizedInputForwardSpeed += Mathf.Sign(moveInput.y) * Time.fixedDeltaTime/inputIncreaseTime;
			characterPhysics.Move(Vector3.zero);
		}

		private void EaseOffInput()
		{
			normalizedInputForwardSpeed =
				Mathf.Lerp(normalizedInputForwardSpeed, 0, Time.fixedDeltaTime / inputDecreaseTime);
			normalizedInputLateralSpeed =
				Mathf.Lerp(normalizedInputLateralSpeed, 0, Time.fixedDeltaTime / inputDecreaseTime);
		}
		
	}
}
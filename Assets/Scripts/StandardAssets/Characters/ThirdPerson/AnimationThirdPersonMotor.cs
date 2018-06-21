using System.Runtime.Serialization.Formatters;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class AnimationThirdPersonMotor : InputThirdPersonMotor
	{
		[SerializeField]
		private float movementTime = 2f;
		
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
			normalizedInputLateralSpeed = 0f;
			if (!characterInput.hasMovementInput)
			{
				normalizedInputForwardSpeed = 0f;
				return;
			}

			normalizedInputForwardSpeed += Time.deltaTime/movementTime;

		}

		protected override void CalculateStrafeMovement()
		{
			if (!characterInput.hasMovementInput)
			{
				normalizedInputLateralSpeed = 0f;
				normalizedInputForwardSpeed = 0f;
				return;
			}
			
			Vector2 moveInput = characterInput.moveInput;
			
			normalizedInputLateralSpeed += Mathf.Sign(moveInput.x) * Time.deltaTime/movementTime;
			normalizedInputForwardSpeed += Mathf.Sign(moveInput.y) * Time.deltaTime/movementTime;
		}
	}
}
using System;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
	public class FirstPersonInput : BaseInput
	{
		public event Action crouchStarted, crouchEnded; 
		
		protected bool isCrouching;

		protected override Vector2 ConditionMoveInput(Vector2 rawMoveInput)
		{
			return rawMoveInput;
		}

		protected override void RegisterAdditionalInputs()
		{
			controls.Movement.crouch.performed += OnCrouchInput;
		}

		protected override void OnSprintInput(InputAction.CallbackContext obj)
		{
			base.OnSprintInput(obj);
			isCrouching = false;
		}

		private void OnCrouchInput(InputAction.CallbackContext obj)
		{
			BroadcastInputAction(ref isCrouching, crouchStarted, crouchEnded);
			isSprinting = false;
		}

		public void ResetInputs()
		{
			isCrouching = false;
			isSprinting = false;
		}
	}
}
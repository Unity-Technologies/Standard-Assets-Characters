using System;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
	public class FirstPersonInput : BaseInput
	{
		public event Action crouchStarted, crouchEnded; 
		
		protected bool isCrouching;
		
		protected override void RegisterAdditionalInputs()
		{
			controls.Movement.crouch.performed += Crouch;
		}

		private void Crouch(InputAction.CallbackContext obj)
		{
			BroadcastInputAction(ref isCrouching, crouchStarted, crouchEnded);
		}
	}
}
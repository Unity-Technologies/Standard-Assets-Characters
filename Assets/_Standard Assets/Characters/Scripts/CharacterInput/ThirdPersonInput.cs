using System;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
	public class ThirdPersonInput : BaseInput, IThirdPersonInput
	{
		public event Action strafeStarted, strafeEnded, recentreCamera; 
		
		protected bool isStrafing;
		
		protected override void RegisterAdditionalInputs()
		{
			controls.Movement.strafe.performed += OnStrafeInput;
			controls.Movement.recentre.performed += OnRecentreInput;
		}

		private void OnRecentreInput(InputAction.CallbackContext obj)
		{
			if (recentreCamera != null)
			{
				recentreCamera();
			}
		}

		private void OnStrafeInput(InputAction.CallbackContext obj)
		{
			BroadcastInputAction(ref isStrafing, strafeStarted, strafeEnded);
		}

		public void ResetSprint()
		{
			isSprinting = false;
		}
	}
}
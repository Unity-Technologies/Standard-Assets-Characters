using System;
using StandardAssets.Characters.Common;
using UnityEngine.InputSystem;


namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Implementation of the Third Person input
	/// </summary>
	public class ThirdPersonInput : CharacterInput, IThirdPersonInput
	{
		/// <summary>
		/// Fired when strafe input is started
		/// </summary>
		public event Action strafeStarted;

		/// <summary>
		/// Fired when the strafe input is ended
		/// </summary>
		public event Action strafeEnded;
		
		/// <summary>
		/// Fired when the recentre camera input is applied
		/// </summary>
		public event Action recentreCamera;

		// Tracks if the character is strafing 
		bool m_IsStrafing;

		/// <summary>
		/// Sets the sprinting state to false
		/// </summary>
		public void ResetSprint()
		{
			isSprinting = false;
		}

		/// <summary>
		/// Registers strafe and recentre inputs.
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
			standardControls.Movement.recentre.performed += OnRecentreInput;
		
			if(UseTouchControls())
			{
				standardControls.Movement.strafeToggle.performed += OnStrafeInput;
				standardControls.Movement.strafeToggle.canceled += OnStrafeInput;
			}
			else
			{
				standardControls.Movement.strafe.performed += OnStrafeInput;
				standardControls.Movement.strafe.canceled += OnStrafeInput;
			}
		}

		// Handles the recentre input 
		void OnRecentreInput(InputAction.CallbackContext context)
		{
			if (recentreCamera != null)
			{
				recentreCamera();
			}
		}

		// Handles the strafe input
		void OnStrafeInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref m_IsStrafing, strafeStarted, strafeEnded);
		}
	}
}
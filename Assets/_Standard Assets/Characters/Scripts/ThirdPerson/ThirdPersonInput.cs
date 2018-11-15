using System;
using StandardAssets.Characters.Common;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

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

		/// <summary>
		/// Tracks if the character is strafing 
		/// </summary>
		bool m_IsStrafing;

		/// <summary>
		/// Registers strafe and recentre inputs.
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
			standardControls.Movement.strafe.performed += OnStrafeInput;
			standardControls.Movement.recentre.performed += OnRecentreInput;
		}

		protected override void RegisterAdditionalTouchInputs()
		{
			touchControls.Movement.strafe.performed += OnStrafeInput;
			touchControls.Movement.recentre.performed += OnRecentreInput;
		}

		/// <summary>
		/// Handles the recentre input 
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		void OnRecentreInput(InputAction.CallbackContext context)
		{
			if (recentreCamera != null)
			{
				recentreCamera();
			}
		}

		/// <summary>
		/// Handles the strafe input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		void OnStrafeInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref m_IsStrafing, strafeStarted, strafeEnded);
		}

		/// <summary>
		/// Sets the sprinting state to false
		/// </summary>
		public void ResetSprint()
		{
			isSprinting = false;
		}
	}
}
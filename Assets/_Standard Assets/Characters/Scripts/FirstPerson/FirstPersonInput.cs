using System;
using StandardAssets.Characters.Common;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Implementation of the First Person input
	/// </summary>
	public class FirstPersonInput : CharacterInput
	{
		/// <summary>
		/// Fired when the crouch is started
		/// </summary>
		public event Action crouchStarted;
		
		/// <summary>
		/// Fired when the crouch is ended
		/// </summary>
		public event Action crouchEnded; 
		
		// Tracks whether the character is crouching or not
		bool m_IsCrouching;

		/// <summary>
		/// Resets the input states
		/// </summary>
		/// <remarks>used by the <see cref="StandardAssets.Characters.FirstPerson.FirstPersonBrain"/> to reset inputs when entering the walking state</remarks>
		public void ResetInputs()
		{
			m_IsCrouching = false;
			isSprinting = false;
		}

		/// <summary>
		/// Registers crouch input
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
			standardControls.Movement.crouch.performed += OnCrouchInput;
			standardControls.Movement.crouch.cancelled += OnCrouchInput;
		}

		/// <summary>
		/// Handles the sprint input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		protected override void OnSprintInput(InputAction.CallbackContext context)
		{
			base.OnSprintInput(context);
			m_IsCrouching = false;
		}

		// Handles the crouch input
		void OnCrouchInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref m_IsCrouching, crouchStarted, crouchEnded);
			isSprinting = false;
		}

	}
}
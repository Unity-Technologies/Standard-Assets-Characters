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
		
		/// <summary>
		/// Tracks whether the character is crouching or not
		/// </summary>
		bool m_IsCrouching;

		/// <summary>
		/// Registers crouch
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
			standardControls.Movement.crouch.performed += OnCrouchInput;
		}

		protected override void RegisterAdditionalTouchInputs()
		{
			touchControls.Movement.crouch.performed += OnCrouchInput;
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

		/// <summary>
		/// Handles the crouch input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		void OnCrouchInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref m_IsCrouching, crouchStarted, crouchEnded);
			isSprinting = false;
		}

		/// <summary>
		/// Resets the input states
		/// </summary>
		/// <remarks>used by the <see cref="StandardAssets.Characters.FirstPerson.FirstPersonBrain"/> to reset inputs when entering the walking state</remarks>
		public void ResetInputs()
		{
			m_IsCrouching = false;
			isSprinting = false;
		}
	}
}
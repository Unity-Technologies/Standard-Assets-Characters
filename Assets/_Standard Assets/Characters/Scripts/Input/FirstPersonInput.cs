using System;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.Input
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
		protected bool isCrouching;

		/// <summary>
		/// Returns the <paramref name="rawMoveInput"/>
		/// </summary>
		/// <param name="rawMoveInput">The move input vector received from the input action</param>
		/// <returns><paramref name="rawMoveInput"/></returns>
		protected override Vector2 ConditionMoveInput(Vector2 rawMoveInput)
		{
			return rawMoveInput;
		}

		/// <summary>
		/// Registers crouch
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
			controls.Movement.crouch.performed += OnCrouchInput;
		}

		/// <summary>
		/// Handles the sprint input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		protected override void OnSprintInput(InputAction.CallbackContext context)
		{
			base.OnSprintInput(context);
			isCrouching = false;
		}

		/// <summary>
		/// Handles the crouch input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		private void OnCrouchInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref isCrouching, crouchStarted, crouchEnded);
			isSprinting = false;
		}

		/// <summary>
		/// Resets the input states
		/// </summary>
		/// <remarks>used by the <see cref="StandardAssets.Characters.FirstPerson.FirstPersonBrain"/> to reset inputs when entering the walking state</remarks>
		public void ResetInputs()
		{
			isCrouching = false;
			isSprinting = false;
		}
	}
}
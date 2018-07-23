using System;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Mechanism of inducing the first person state modification
	/// e.g. Used to set up crouching
	/// </summary>
	[Serializable]
	public class FirstPersonMovementModification
	{
		/// <summary>
		/// The input
		/// </summary>
		[SerializeField]
		protected CompoundInputResponse inputs;

		/// <summary>
		/// The state
		/// </summary>
		[SerializeField] 
		protected FirstPersonMovementProperties movementProperties;

		/// <summary>
		/// The controller
		/// </summary>
		private FirstPersonBrain brain;

		/// <summary>
		/// Initializes the modification
		/// </summary>
		/// <param name="brainToUse"></param>
		public void Init(FirstPersonBrain brainToUse)
		{
			brain = brainToUse;
			inputs.Init();
			inputs.started += OnStateChange;
			inputs.ended += OnStateReset;
		}

		private void OnStateChange()
		{
			brain.EnterNewState(movementProperties);
		}

		private void OnStateReset()
		{
			brain.ResetState();
		}
		
		
		/// <summary>
		/// Returns the movement properties for use in the UI
		/// Setting and viewing speeds for movement properties
		/// </summary>
		/// <returns></returns>
		public FirstPersonMovementProperties GetMovementProperty()
		{
			return movementProperties;
		}
	}
}
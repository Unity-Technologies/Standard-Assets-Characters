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
		/// The Input Response used to trigger the modification
		/// </summary>
		[SerializeField, Tooltip("The Input Response used to trigger the modification")]
		protected InputResponse inputResponse;

		/// <summary>
		/// The movement properties used in this modification
		/// </summary>
		[SerializeField, Tooltip("The movement properties used in this modification")] 
		protected FirstPersonMovementProperties movementProperties;

		/// <summary>
		/// The controller
		/// </summary>
		private FirstPersonBrain brain;

		/// <summary>
		/// Initializes the modification
		/// </summary>
		/// <param name="brainToUse">The brain is passed into the modification</param>
		public void Init(FirstPersonBrain brainToUse)
		{
			brain = brainToUse;
			inputResponse.Init();
			inputResponse.started += OnStateChange;
			inputResponse.ended += OnStateReset;
		}

		/// <summary>
		/// Uses the brain to change state
		/// </summary>
		private void OnStateChange()
		{
			brain.EnterNewState(movementProperties);
		}

		/// <summary>
		/// Uses the brain to reset the state
		/// </summary>
		private void OnStateReset()
		{
			brain.ResetState();
		}	
		
		/// <summary>
		/// Returns the movement properties for use in the UI
		/// Setting and viewing speeds for movement properties
		/// </summary>
		/// <returns>movement properties for use in the UI</returns>
		public FirstPersonMovementProperties GetMovementProperty()
		{
			return movementProperties;
		}
	}
}
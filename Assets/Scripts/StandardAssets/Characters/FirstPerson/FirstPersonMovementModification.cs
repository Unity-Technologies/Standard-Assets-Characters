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
		private InputResponse[] inputs;

		/// <summary>
		/// The state
		/// </summary>
		[SerializeField] private FirstPersonMovementProperties movementProperties;

		/// <summary>
		/// The controller
		/// </summary>
		private FirstPersonController controller;

		/// <summary>
		/// Initializes the modification
		/// </summary>
		/// <param name="controllerToUse"></param>
		public void Init(FirstPersonController controllerToUse)
		{
			controller = controllerToUse;
			foreach (var input in inputs)
			{
				input.Init();
                input.started += OnStateChange;
       			input.ended += OnStateReset;
			}
		}

		private void OnStateChange()
		{
			controller.EnterNewState(movementProperties);
		}

		private void OnStateReset()
		{
			controller.ResetState();
		}
		
		
		/// <summary>
		/// Returns the movement properties for use in the UI
		/// Setting and viewing speeds for movement properties
		/// </summary>
		/// <returns></returns>
		public FirstPersonMovementProperties getMovementProperty()
		{
			return movementProperties;
		}
	}
}
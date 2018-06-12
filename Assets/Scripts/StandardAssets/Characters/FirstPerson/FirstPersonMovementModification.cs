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
		private InputResponse input;
		
		/// <summary>
		/// The state
		/// </summary>
		[SerializeField]
		private FirstPersonMovementProperties movementProperties;

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
			input.Init();
			input.started += OnStateChange;
			input.ended += OnStateReset;
		}

		public void Tick()
		{
			input.Tick();
		}

		private void OnStateChange()
		{
			controller.EnterNewState(movementProperties);
		}

		private void OnStateReset()
		{
			controller.ResetState();
		}
	}
}
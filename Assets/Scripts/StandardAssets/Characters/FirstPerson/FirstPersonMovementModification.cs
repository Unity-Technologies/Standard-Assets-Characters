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
		public InputResponse input;
		
		/// <summary>
		/// The state
		/// </summary>
		public FirstPersonMovementProperties movementProperties;

		/// <summary>
		/// The controller
		/// </summary>
		FirstPersonController m_Controller;

		/// <summary>
		/// Initializes the modification
		/// </summary>
		/// <param name="controller"></param>
		public void Init(FirstPersonController controller)
		{
			m_Controller = controller;
			input.Init();
			input.started += OnStateChange;
			input.ended += OnStateReset;
		}

		public void Tick()
		{
			input.Tick();
		}

		void OnStateChange()
		{
			m_Controller.EnterNewState(movementProperties);
		}

		void OnStateReset()
		{
			m_Controller.ResetState();
		}
	}
}
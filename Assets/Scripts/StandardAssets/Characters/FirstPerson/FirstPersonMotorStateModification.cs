using System;
using StandardAssets.Characters.Input;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Mechanism of inducing the first person state modification
	/// e.g. Used to set up crouching
	/// </summary>
	public class FirstPersonMotorStateModification : MonoBehaviour
	{
		/// <summary>
		/// The input
		/// </summary>
		public InputResponse input;
		
		/// <summary>
		/// The state
		/// </summary>
		public FirstPersonMotorState state;

		/// <summary>
		/// The controller
		/// </summary>
		FirstPersonMotorStateModificationController m_Controller;

		/// <summary>
		/// Initializes the modification
		/// </summary>
		/// <param name="controller"></param>
		public void Init(FirstPersonMotorStateModificationController controller)
		{
			m_Controller = controller;
			input.Init();
			input.started += OnStateChange;
			input.ended += OnStateReset;
		}

		void Update()
		{
			input.Tick();
		}

		void OnStateChange()
		{
			m_Controller.ChangeState(state);
		}

		void OnStateReset()
		{
			m_Controller.ResetState();
		}
	}
}
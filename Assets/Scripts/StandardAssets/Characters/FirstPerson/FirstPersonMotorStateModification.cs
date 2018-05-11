using System;
using StandardAssets.Characters.Input;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	public class FirstPersonMotorStateModification : MonoBehaviour
	{
		public InputResponse input;
		public FirstPersonMotorState state;

		FirstPersonMotorStateModificationController m_Controller;

		public void Init(FirstPersonMotorStateModificationController controller)
		{
			m_Controller = controller;
			input.Init();
			input.enabled += OnStateChange;
			input.disabled += OnStateReset;
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
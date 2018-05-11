using System;
using StandardAssets.Characters.Input;

namespace StandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class FirstPersonMotorStateModification
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

		public void Tick()
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
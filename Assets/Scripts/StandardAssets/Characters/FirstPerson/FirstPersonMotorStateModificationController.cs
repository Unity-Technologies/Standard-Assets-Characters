using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(FirstPersonMotor))]
	public class FirstPersonMotorStateModificationController : MonoBehaviour
	{
		public FirstPersonMotorStateModification[] modifiers;
		
		FirstPersonMotor m_Motor;
		FirstPersonMotorState m_PrevState;

		public void ChangeState(FirstPersonMotorState newState)
		{
			m_PrevState = m_Motor.currentMotorState;
			m_Motor.ChangeState(newState);
		}

		public void ResetState()
		{
			m_Motor.ChangeState(m_PrevState);
		}
		
		void Awake()
		{
			m_Motor = GetComponent<FirstPersonMotor>();
			foreach (FirstPersonMotorStateModification modifier in modifiers)
			{
				modifier.Init(this);
			}
		}
	}
}
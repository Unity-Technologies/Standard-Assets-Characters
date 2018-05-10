using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(FirstPersonMotor))]
	public abstract class FirstPersonMotorStateChange : MonoBehaviour
	{
		public FirstPersonMotorState state;
		
		FirstPersonMotor m_Motor;

		FirstPersonMotorState m_PrevState;

		protected virtual void Awake()
		{
			m_Motor = GetComponent<FirstPersonMotor>();
		}

		protected void ChangeState()
		{
			m_PrevState = m_Motor.currentMotorState;
			m_Motor.ChangeState(state);
		}

		protected void ResetState()
		{
			m_Motor.ChangeState(m_PrevState);
		}
	}
}
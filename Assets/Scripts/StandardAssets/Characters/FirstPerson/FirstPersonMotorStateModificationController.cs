using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(FirstPersonMotor))]
	public class FirstPersonMotorStateModificationController : MonoBehaviour
	{
		public FirstPersonMotorStateModification[] modifiers;
		
		FirstPersonMotor m_Motor;
		Stack<FirstPersonMotorState> m_PrevStates = new Stack<FirstPersonMotorState>();

		public void ChangeState(FirstPersonMotorState newState)
		{
			m_PrevStates.Push(m_Motor.currentMotorState);
			m_Motor.ChangeState(newState);
		}

		public void ResetState()
		{
			if (m_PrevStates.Count > 0)
			{
				m_Motor.ChangeState(m_PrevStates.Pop());
			}
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
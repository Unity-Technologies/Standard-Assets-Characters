using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// Manages the state modifications
	/// </summary>
	[RequireComponent(typeof(FirstPersonMotor))]
	public class FirstPersonMotorStateModificationController : MonoBehaviour
	{
		/// <summary>
		/// List of possible state modifiers
		/// </summary>
		public FirstPersonMotorStateModification[] modifiers;
		
		/// <summary>
		/// The motor
		/// </summary>
		FirstPersonMotor m_Motor;
		
		/// <summary>
		/// A stack of states which allows us to revert through previous states
		/// </summary>
		Stack<FirstPersonMotorState> m_PrevStates = new Stack<FirstPersonMotorState>();

		/// <summary>
		/// Change state to the new state and adds to previous state stack
		/// </summary>
		/// <param name="newState"></param>
		public void ChangeState(FirstPersonMotorState newState)
		{
			m_PrevStates.Push(m_Motor.currentMotorState);
			m_Motor.ChangeState(newState);
		}

		/// <summary>
		/// Resets state to previous state
		/// </summary>
		public void ResetState()
		{
			if (m_PrevStates.Count > 0)
			{
				m_Motor.ChangeState(m_PrevStates.Pop());
			}
		}
		
		/// <summary>
		/// Initialize the states
		/// </summary>
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
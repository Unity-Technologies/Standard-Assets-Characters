using StandardAssets.Characters.Input;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace StandardAssets.Characters.FirstPerson
{
	[RequireComponent(typeof(IPhysics))]
	[RequireComponent(typeof(IFirstPersonInput))]
	public class FirstPersonMotor : MonoBehaviour
	{
		public FirstPersonMotorState startingMotorState;

		protected FirstPersonMotorState m_CurrentMotorState;

		protected IPhysics m_Physics;

		protected IFirstPersonInput m_Input;
		
		protected virtual void Awake()
		{
			m_Physics = GetComponent<IPhysics>();
			m_Input = GetComponent<IFirstPersonInput>();
			ChangeState(startingMotorState);
		}

		void FixedUpdate()
		{
			Move();
		}

		void Move()
		{
			if (startingMotorState == null)
			{
				return;
			}

			if (m_Input.isMoveInput)
			{
				Accelerate();
			}
			else
			{
				Decelerate();
			}
		}	

		void Accelerate()
		{
			throw new System.NotImplementedException();
		}
		
		void Decelerate()
		{
			throw new System.NotImplementedException();
		}

		public virtual void ChangeState(FirstPersonMotorState newState)
		{
			if (newState == null)
			{
				return;
			}
			
			if (m_CurrentMotorState != null)
			{
				m_CurrentMotorState.ExitState();
			}

			m_CurrentMotorState = newState;
			m_CurrentMotorState.EnterState();
		}
	}
}
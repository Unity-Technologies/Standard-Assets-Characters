using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(IThirdPersonMotor))]
	public class ThirdPersonMotorStateMachine : MonoBehaviour, IThirdPersonMotorStateMachine
	{
		public float walkThreshold = 0.1f, runThreshold = 0.6f;
		
		public Action idling { get; set; }
		public Action walking { get; set; }
		public Action running { get; set; }

		IThirdPersonMotor m_Motor;

		MovementState m_PreviousMovementState = MovementState.Idle;

		void Awake()
		{
			m_Motor = GetComponent<IThirdPersonMotor>();
		}

		void Update()
		{
			float forwardSpeed = Mathf.Abs(m_Motor.forwardSpeed);
			if (forwardSpeed > runThreshold)
			{
				SetRun();
				return;
			}
			
			if(forwardSpeed > walkThreshold)
			{
				SetWalk();
				return;
			}

			SetIdle();
		}

		void SetIdle()
		{
			SetState(MovementState.Idle, idling);
		}

		void SetWalk()
		{
			SetState(MovementState.Walk, walking);
		}

		void SetRun()
		{
			SetState(MovementState.Run, running);
		}

		void SetState(MovementState newState, Action stateChange)
		{
			if (m_PreviousMovementState != newState && stateChange != null)
			{
				stateChange();
			}

			m_PreviousMovementState = newState;
		}
	}

	public enum MovementState
	{
		Idle,
		Walk,
		Run
	}
}
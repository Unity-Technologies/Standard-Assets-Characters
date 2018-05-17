using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <inheritdoc />
	/// Simple implementation that maps the forward speed to a state
	[RequireComponent(typeof(IThirdPersonMotor))]
	public class ThirdPersonMotorStateMachine : MonoBehaviour, IThirdPersonMotorStateMachine
	{
		/// <summary>
		/// Thresholds for movement states
		/// </summary>
		public float walkThreshold = 0.1f, runThreshold = 0.6f;
		
		/// <inheritdoc />
		public Action idling { get; set; }
		
		/// <inheritdoc />
		public Action walking { get; set; }
		
		/// <inheritdoc />
		public Action running { get; set; }

		//The required motor
		IThirdPersonMotor m_Motor;

		//The current movement state
		MovementState m_PreviousMovementState = MovementState.Idle;

		/// <summary>
		/// Gets the motor
		/// </summary>
		void Awake()
		{
			m_Motor = GetComponent<IThirdPersonMotor>();
		}

		/// <summary>
		/// Check states based on the motor speed
		/// </summary>
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

		/// <summary>
		/// Wrapper setting state to idle
		/// </summary>
		void SetIdle()
		{
			SetState(MovementState.Idle, idling);
		}

		/// <summary>
		/// Wrapper setting state to walking
		/// </summary>
		void SetWalk()
		{
			SetState(MovementState.Walk, walking);
		}

		/// <summary>
		/// Wrapper setting state to running
		/// </summary>
		void SetRun()
		{
			SetState(MovementState.Run, running);
		}

		/// <summary>
		/// Helper function for handling state change
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="stateChange"></param>
		void SetState(MovementState newState, Action stateChange)
		{
			if (m_PreviousMovementState != newState && stateChange != null)
			{
				stateChange();
			}

			m_PreviousMovementState = newState;
		}
	}

	/// <summary>
	/// Different movement states
	/// </summary>
	public enum MovementState
	{
		Idle,
		Walk,
		Run
	}
}
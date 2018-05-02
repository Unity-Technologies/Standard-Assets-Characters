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

		protected float currentSpeed = 0f, movementTime = 0f;

		protected bool prevIsMoveInput = false;
		
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
				if (!prevIsMoveInput)
				{
					movementTime = 0f;
				}
				Accelerate();
			}
			else
			{
				if (prevIsMoveInput)
				{
					movementTime = 0f;
				}
				Decelerate();
			}


			if (currentSpeed < Mathf.Epsilon)
			{
				return;
			}
			
			Vector2 input = m_Input.moveInput;
			if (input.sqrMagnitude > 1)
			{
				input.Normalize();
			}

			Vector3 forward = transform.forward * m_Input.moveInput.y;
			Vector3 sideways = transform.right * m_Input.moveInput.x;
			
			m_Physics.Move((forward + sideways) * currentSpeed * Time.deltaTime);

			prevIsMoveInput = m_Input.isMoveInput;
		}	

		void Accelerate()
		{
			movementTime += Time.fixedDeltaTime;
			movementTime = Mathf.Clamp(movementTime, 0f, m_CurrentMotorState.acceleration.maxValue);
			currentSpeed = m_CurrentMotorState.acceleration.Evaluate(movementTime) * m_CurrentMotorState.maxSpeed;
		}
		
		void Decelerate()
		{
			currentSpeed = 0f;
		}

		void ClampCurrentSpeed()
		{
			currentSpeed = Mathf.Clamp(currentSpeed, 0f, m_CurrentMotorState.maxSpeed);
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
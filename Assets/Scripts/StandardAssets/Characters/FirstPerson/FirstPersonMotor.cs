using StandardAssets.Characters.Input;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// The main controller of first person character
	/// Ties together the input and physics implementations
	/// </summary>
	[RequireComponent(typeof(IPhysics))]
	[RequireComponent(typeof(IInput))]
	public class FirstPersonMotor : MonoBehaviour
	{
		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		public FirstPersonMotorState startingMotorState;

		/// <summary>
		/// The Physic implementation used to do the movement
		/// e.g. CharacterController or Rigidbody (or New C# CharacterController analog)
		/// </summary>
		protected IPhysics m_Physics;

		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		protected IInput m_Input;

		/// <summary>
		/// The current movement properties
		/// </summary>
		protected float currentSpeed = 0f, movementTime = 0f;

		/// <summary>
		/// A check to see if input was previous being applied
		/// </summary>
		protected bool prevIsMoveInput = false;
		
		/// <summary>
		/// The current motor state - controls how the character moves in different states
		/// </summary>
		public FirstPersonMotorState currentMotorState { get; protected set; }
		
		/// <summary>
		/// Get the attached implementations on wake
		/// </summary>
		protected virtual void Awake()
		{
			m_Physics = GetComponent<IPhysics>();
			m_Input = GetComponent<IInput>();
			ChangeState(startingMotorState);
		}

		/// <summary>
		/// Handles movement on Physics update
		/// </summary>
		void FixedUpdate()
		{
			Move();
		}

		/// <summary>
		/// State based movement
		/// </summary>
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

		/// <summary>
		/// Calculates current speed based on acceleration anim curve
		/// </summary>
		void Accelerate()
		{
			movementTime += Time.fixedDeltaTime;
			movementTime = Mathf.Clamp(movementTime, 0f, currentMotorState.acceleration.maxValue);
			currentSpeed = currentMotorState.acceleration.Evaluate(movementTime) * currentMotorState.maxSpeed;
		}
		
		/// <summary>
		/// Calculates the current speed based on the deceleration anim curve
		/// </summary>
		void Decelerate()
		{
			//TODO: implement
			currentSpeed = 0f;
		}

		/// <summary>
		/// Clamps the current speed
		/// </summary>
		void ClampCurrentSpeed()
		{
			currentSpeed = Mathf.Clamp(currentSpeed, 0f, currentMotorState.maxSpeed);
		}

		/// <summary>
		/// Changes the current motor state and play events associated with state change
		/// </summary>
		/// <param name="newState"></param>
		public virtual void ChangeState(FirstPersonMotorState newState)
		{
			if (newState == null)
			{
				return;
			}
			
			if (currentMotorState != null)
			{
				currentMotorState.ExitState();
			}

			currentMotorState = newState;
			currentMotorState.EnterState();
		}
	}
}
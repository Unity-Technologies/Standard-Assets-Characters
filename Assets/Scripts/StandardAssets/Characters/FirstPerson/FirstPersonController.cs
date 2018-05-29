using System.Collections.Generic;
using StandardAssets.Characters.Input;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// The main controller of first person character
	/// Ties together the input and physics implementations
	/// </summary>
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public class FirstPersonController : MonoBehaviour
	{
		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		public FirstPersonMovementProperties startingMovementProperties;
		
		/// <summary>
		/// List of possible state modifiers
		/// </summary>
		public FirstPersonMovementModification[] movementModifiers;

		/// <summary>
		/// The Physic implementation used to do the movement
		/// e.g. CharacterController or Rigidbody (or New C# CharacterController analog)
		/// </summary>
		protected ICharacterPhysics m_CharacterPhysics;

		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		protected ICharacterInput m_CharacterInput;

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
		public FirstPersonMovementProperties currentMovementProperties { get; protected set; }
		
		/// <summary>
		/// A stack of states which allows us to revert through previous states
		/// </summary>
		Stack<FirstPersonMovementProperties> m_PrevStates = new Stack<FirstPersonMovementProperties>();
		
		/// <summary>
		/// Get the attached implementations on wake
		/// </summary>
		protected virtual void Awake()
		{
			m_CharacterPhysics = GetComponent<ICharacterPhysics>();
			m_CharacterInput = GetComponent<ICharacterInput>();
			m_CharacterInput.jump += Jump;
			foreach (FirstPersonMovementModification modifier in movementModifiers)
			{
				modifier.Init(this);
			}
			ChangeState(startingMovementProperties);
		}

		/// <summary>
		/// Handles jumping
		/// </summary>
		void Jump()
		{
			if (m_CharacterPhysics.isGrounded && currentMovementProperties.canJump)
			{
				m_CharacterPhysics.Jump(currentMovementProperties.jumpSpeed);
			}	
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
			if (startingMovementProperties == null)
			{
				return;
			}

			if (m_CharacterInput.isMoveInput)
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

				Stop();
			}

			Vector2 input = m_CharacterInput.moveInput;
			if (input.sqrMagnitude > 1)
			{
				input.Normalize();
			}

			Vector3 forward = transform.forward * m_CharacterInput.moveInput.y;
			Vector3 sideways = transform.right * m_CharacterInput.moveInput.x;
			
			m_CharacterPhysics.Move((forward + sideways) * currentSpeed * Time.deltaTime);

			prevIsMoveInput = m_CharacterInput.isMoveInput;
		}	

		/// <summary>
		/// Calculates current speed based on acceleration anim curve
		/// </summary>
		void Accelerate()
		{
			movementTime += Time.fixedDeltaTime;
			movementTime = Mathf.Clamp(movementTime, 0f, currentMovementProperties.acceleration.maxValue);
			currentSpeed = currentMovementProperties.acceleration.Evaluate(movementTime) * currentMovementProperties.maxSpeed;
		}
		
		/// <summary>
		/// Stops the movement
		/// </summary>
		void Stop()
		{
			currentSpeed = 0f;
		}

		/// <summary>
		/// Clamps the current speed
		/// </summary>
		void ClampCurrentSpeed()
		{
			currentSpeed = Mathf.Clamp(currentSpeed, 0f, currentMovementProperties.maxSpeed);
		}
		
		/// <summary>
		/// Ensures that the current speed doesn't rapidly increase
		/// </summary>
		void CalculateMovementTimeFromCurrentSpeed()
		{
		}
		
		/// <summary>
		/// Change state to the new state and adds to previous state stack
		/// </summary>
		/// <param name="newState"></param>
		public void EnterNewState(FirstPersonMovementProperties newState)
		{
			m_PrevStates.Push(currentMovementProperties);
			ChangeState(newState);
		}

		/// <summary>
		/// Resets state to previous state
		/// </summary>
		public void ResetState()
		{
			if (m_PrevStates.Count > 0)
			{
				ChangeState(m_PrevStates.Pop());
			}
		}
		
		/// <summary>
		/// Changes the current motor state and play events associated with state change
		/// </summary>
		/// <param name="newState"></param>
		protected virtual void ChangeState(FirstPersonMovementProperties newState)
		{
			if (newState == null)
			{
				return;
			}
			
			if (currentMovementProperties != null)
			{
				currentMovementProperties.ExitState();
			}

			currentMovementProperties = newState;
			currentMovementProperties.EnterState();
			CalculateMovementTimeFromCurrentSpeed();
		}

	}
}
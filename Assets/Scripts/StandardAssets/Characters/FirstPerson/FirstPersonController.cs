using System.Collections.Generic;
using StandardAssets.Characters.CharacterInput;
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
		[SerializeField]
		private FirstPersonMovementProperties startingMovementProperties;
		
		/// <summary>
		/// List of possible state modifiers
		/// </summary>
		[SerializeField]
		private FirstPersonMovementModification[] movementModifiers;

		/// <summary>
		/// The Physic implementation used to do the movement
		/// e.g. CharacterController or Rigidbody (or New C# CharacterController analog)
		/// </summary>
		private ICharacterPhysics characterPhysics;

		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		private ICharacterInput characterInput;

		/// <summary>
		/// The current movement properties
		/// </summary>
		private float currentSpeed;

		/// <summary>
		/// The current movement properties
		/// </summary>
		private float movementTime;

		/// <summary>
		/// A check to see if input was previous being applied
		/// </summary>
		private bool previouslyHasInput;
		
		/// <summary>
		/// A stack of states which allows us to revert through previous states
		/// </summary>
		private readonly Stack<FirstPersonMovementProperties> prevStates = new Stack<FirstPersonMovementProperties>();
		
		/// <summary>
		/// The current motor state - controls how the character moves in different states
		/// </summary>
		public FirstPersonMovementProperties currentMovementProperties { get; protected set; }

		/// <summary>
		/// Get the attached implementations on wake
		/// </summary>
		protected virtual void Awake()
		{
			characterPhysics = GetComponent<ICharacterPhysics>();
			characterInput = GetComponent<ICharacterInput>();
			foreach (FirstPersonMovementModification modifier in movementModifiers)
			{
				modifier.Init(this);
			}

			ChangeState(startingMovementProperties);
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		private void OnEnable()
		{
			characterInput.jumpPressed += OnJumpPressed;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		private void OnDisable()
		{
			if (characterInput == null)
			{
				return;
			}
			
			characterInput.jumpPressed -= OnJumpPressed;
		}

		/// <summary>
		/// Handles jumping
		/// </summary>
		private void OnJumpPressed()
		{
			if (characterPhysics.isGrounded && currentMovementProperties.canJump)
			{
				characterPhysics.SetJumpVelocity(currentMovementProperties.jumpSpeed);
			}	
		}

		/// <summary>
		/// Handles movement on Physics update
		/// </summary>
		private void FixedUpdate()
		{
			Move();
		}

		/// <summary>
		/// State based movement
		/// </summary>
		private void Move()
		{
			if (startingMovementProperties == null)
			{
				return;
			}

			if (characterInput.hasMovementInput)
			{
				if (!previouslyHasInput)
				{
					movementTime = 0f;
				}
				Accelerate();
			}
			else
			{
				if (previouslyHasInput)
				{
					movementTime = 0f;
				}

				Stop();
			}

			Vector2 input = characterInput.moveInput;
			if (input.sqrMagnitude > 1)
			{
				input.Normalize();
			}

			Vector3 forward = transform.forward * characterInput.moveInput.y;
			Vector3 sideways = transform.right * characterInput.moveInput.x;
			
			characterPhysics.Move((forward + sideways) * currentSpeed * Time.deltaTime);

			previouslyHasInput = characterInput.hasMovementInput;
		}	

		/// <summary>
		/// Calculates current speed based on acceleration anim curve
		/// </summary>
		private void Accelerate()
		{
			movementTime += Time.fixedDeltaTime;
			movementTime = Mathf.Clamp(movementTime, 0f, currentMovementProperties.accelerationCurve.maxValue);
			currentSpeed = currentMovementProperties.accelerationCurve.Evaluate(movementTime) * currentMovementProperties.maximumSpeed;
		}
		
		/// <summary>
		/// Stops the movement
		/// </summary>
		private void Stop()
		{
			currentSpeed = 0f;
		}

		/// <summary>
		/// Clamps the current speed
		/// </summary>
		private void ClampCurrentSpeed()
		{
			currentSpeed = Mathf.Clamp(currentSpeed, 0f, currentMovementProperties.maximumSpeed);
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
		}

		/// <summary>
		/// Change state to the new state and adds to previous state stack
		/// </summary>
		/// <param name="newState"></param>
		public void EnterNewState(FirstPersonMovementProperties newState)
		{
			prevStates.Push(currentMovementProperties);
			ChangeState(newState);
		}

		/// <summary>
		/// Resets state to previous state
		/// </summary>
		public void ResetState()
		{
			if (prevStates.Count > 0)
			{
				ChangeState(prevStates.Pop());
			}
		}
	}
}
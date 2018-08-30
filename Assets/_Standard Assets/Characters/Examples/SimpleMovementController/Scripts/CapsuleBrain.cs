using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	[RequireComponent(typeof(ICharacterPhysics))]
	[RequireComponent(typeof(ICharacterInput))]
	public class CapsuleBrain : CharacterBrain
	{
		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[SerializeField]
		protected CapsuleMovementProperties startingMovementProperties;

		[SerializeField] 
		protected float turnSpeed = 300f;
			
		/// <summary>
		/// The current motor state - controls how the character moves in different states
		/// </summary>
		public CapsuleMovementProperties currentMovementProperties { get; protected set; }
	   
		
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

		private MovementEventHandler _movementEventHandler;

		public override MovementEventHandler movementEventHandler
		{
			get { return _movementEventHandler; }
		}

		public override float targetYRotation { get; set; }

		protected override void Awake()
		{
			base.Awake();
			ChangeState(startingMovementProperties);
		}

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
		/// Handles camera rotation
		/// </summary>
		private void Update()
		{
			Quaternion targetRotation = CalculateTargetRotation();
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

			targetYRotation = targetRotation.eulerAngles.y;
		}
		
		
		protected virtual Quaternion CalculateTargetRotation()
		{
			Vector3 flatForward = Camera.main.transform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection =
				new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		/// <summary>
		/// Handles jumping
		/// </summary>
		private void OnJumpPressed()
		{
			if (characterPhysics.isGrounded && currentMovementProperties.canJump)
			{
				characterPhysics.SetJumpVelocity(currentMovementProperties.jumpingSpeed);
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
		
			Vector3 forward = transform.forward * input.magnitude;
			Vector3 sideways = Vector3.zero;
			
			characterPhysics.Move((forward + sideways) * currentSpeed * Time.fixedDeltaTime, Time.fixedDeltaTime);

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
		protected virtual void ChangeState(CapsuleMovementProperties newState)
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
		public void EnterNewState(CapsuleMovementProperties newState)
		{
			ChangeState(newState);
		}

		/// <summary>
		/// Resets state to previous state
		/// </summary>
		public void ResetState()
		{
			ChangeState(startingMovementProperties);
			
		}
	}
}
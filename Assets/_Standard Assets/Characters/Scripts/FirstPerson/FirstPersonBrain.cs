using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
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
	public class FirstPersonBrain : CharacterBrain
	{
		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[SerializeField]
		protected FirstPersonMovementProperties startingMovementProperties;

		/// <summary>
		/// List of possible state modifiers
		/// </summary>
		[SerializeField] 
		protected FirstPersonMovementModification[] movementModifiers;
		
		/// <summary>
		/// Main Camera that is using the POV camera
		/// </summary>
		[SerializeField]
		protected Camera mainCamera;

		[SerializeField]
		protected FirstPersonMovementEventHandler firstPersonMovementEventHandler;
		
		[SerializeField]
		protected CameraAnimationManager cameraAnimations;
		
		public CameraAnimationManager cameraAnimationManager
		{
			get { return cameraAnimations; }
		}
		
		/// <summary>
		/// Exposes the movement properties array for use in UI 
		/// </summary>
		public FirstPersonMovementModification[] exposedMovementModifiers
		{
			get { return movementModifiers; }
		}

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

		protected FirstPersonMovementProperties[] allMovement;

		public FirstPersonMovementProperties newMovementProperties;

		/// <summary>
		/// The current motor state - controls how the character moves in different states
		/// </summary>
		public FirstPersonMovementProperties currentMovementProperties { get; protected set; }
		
		public override MovementEventHandler movementEventHandler
		{
			get { return firstPersonMovementEventHandler; }
		}

		public override float targetYRotation { get; set; }

		public FirstPersonMovementProperties[] allMovementProperties
		{
			get
			{
				if (allMovement == null)
				{
					allMovement = new FirstPersonMovementProperties[movementModifiers.Length + 1];
					allMovement[0] = startingMovementProperties;
					int i = 0;
					foreach (FirstPersonMovementModification modifier in movementModifiers)
					{
						i++;
						modifier.Init(this);
						allMovementProperties[i] = modifier.GetMovementProperty();
					}
				}
				
				return allMovement;
			}
		}
		
		protected void SetAnimation(string animation)
		{
			if (cameraAnimations == null)
			{
				Debug.LogWarning("No camera animation manager setup");
				return;
			}
			
			cameraAnimations.SetAnimation(animation);
		}

		/// <summary>
		/// Get the attached implementations on wake
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			firstPersonMovementEventHandler.Init(transform, characterPhysics);
			if (mainCamera == null)
			{
				mainCamera = Camera.main;
			}
			ChangeState(startingMovementProperties);
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		private void OnEnable()
		{
			characterInput.jumpPressed += OnJumpPressed;
			firstPersonMovementEventHandler.Subscribe();

			foreach (FirstPersonMovementProperties movementProperties in allMovementProperties)
			{
				movementProperties.enterState += SetAnimation;
			}

			characterPhysics.landed += OnLanded;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		private void OnDisable()
		{
			firstPersonMovementEventHandler.Unsubscribe();
			if (characterInput == null)
			{
				return;
			}
			
			characterInput.jumpPressed -= OnJumpPressed;
			
			foreach (FirstPersonMovementProperties movementProperties in allMovementProperties)
			{
				movementProperties.enterState -= SetAnimation;
			}
			
			characterPhysics.landed -= OnLanded;
		}
		
		private void OnLanded()
		{
			SetNewMovementProperties();
		}

		private void SetNewMovementProperties()
		{
			if (currentMovementProperties != null)
			{
				currentMovementProperties.ExitState();
			}

			currentMovementProperties = newMovementProperties;
			currentMovementProperties.EnterState();
		}

		/// <summary>
		/// Handles camera rotation
		/// </summary>
		private void Update()
		{
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = mainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
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
			firstPersonMovementEventHandler.Tick();
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
		
			Vector3 forward = transform.forward * input.y;
			Vector3 sideways = transform.right * input.x;
			
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
		protected virtual void ChangeState(FirstPersonMovementProperties newState)
		{
			if (newState == null)
			{
				return;
			}

			newMovementProperties = newState;
			
			if (characterPhysics.isGrounded)
			{
				SetNewMovementProperties();
			}
			
			firstPersonMovementEventHandler.AdjustAudioTriggerThreshold(newState.strideLenthDistance);
		}
		
		

		/// <summary>
		/// Change state to the new state and adds to previous state stack
		/// </summary>
		/// <param name="newState"></param>
		public void EnterNewState(FirstPersonMovementProperties newState)
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
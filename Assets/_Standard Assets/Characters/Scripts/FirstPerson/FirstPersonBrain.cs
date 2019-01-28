using System;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// The main controller of first person character
	/// Ties together the input and physics implementations
	/// </summary>
	[RequireComponent(typeof(FirstPersonInput))]
	public class FirstPersonBrain : CharacterBrain
	{
		// Movement state change events
		public event Action startWalking, startCrouching, startSprinting;
		
		/// <summary>
		/// Stores movement properties for the different states - e.g. walk
		/// </summary>
		[Serializable]
		public struct MovementProperties
		{
			[SerializeField, Tooltip("Maximum movement speed of the character in world units per second"), Range(0.1f, 20f)]
			float m_MaxSpeed;

			[SerializeField, Tooltip("Initial Y velocity of a Jump in world units per second"), Range(0f, 10f)]
			float m_JumpSpeed;

			[SerializeField, Tooltip("Distance, in world units, that is considered a stride"), Range(0f, 1f)]
			float m_StrideLength;

			/// <summary>
			/// Gets if the character can jump
			/// </summary>
			public bool canJump { get { return m_JumpSpeed > 0f; } }

			/// <summary>
			/// Gets the character's maximum movement speed in world units per second
			/// </summary>
			public float maxSpeed { get { return m_MaxSpeed; } }

			/// <summary>
			/// Gets the character's initial Y velocity of a jump in world units per second
			/// </summary>
			public float jumpSpeed { get { return m_JumpSpeed; } }

			/// <summary>
			/// Gets the character's stride length in world units
			/// </summary>
			public float strideLength { get { return m_StrideLength; } }
		}

		[SerializeField, Tooltip("Movement properties of the character while walking")]
		MovementProperties m_Walking;

		[SerializeField, Tooltip("Movement properties of the character while sprinting")]
		MovementProperties m_Sprinting;

		[SerializeField, Tooltip("Movement properties of the character while crouching")]
		MovementProperties m_Crouching;

		[SerializeField, Tooltip("Management of movement effects e.g. footsteps")]
		FirstPersonMovementEventHandler m_MovementEffects;

		// The current movement properties
		float m_CurrentSpeed;

		// Backing field to prevent the currentProperties from being null
		MovementProperties m_CurrentMovementProperties;

		// Stores the new movement properties if the character tries to change movement state in mid-air
		MovementProperties m_NewMovementProperties;
		
		// Cached instance of Camera.main
		Camera m_MainCamera;

		// Backing field for lazily loading the character input
		FirstPersonInput m_Input;

		/// <summary>
		/// Returns the characters normalized speed
		/// </summary>
		public override float normalizedForwardSpeed
		{
			get
			{
				var maxSpeed = m_CurrentMovementProperties.maxSpeed;
				if (maxSpeed <= 0)
				{
					return 1;
				}

				return m_CurrentSpeed / maxSpeed;
			}
		}

		/// <summary>
		/// Gets the target Y rotation of the character
		/// </summary>
		public override float targetYRotation { get; set; }

		// Lazily loads the FirstPersonInput
		FirstPersonInput characterInput
		{
			get
			{
				if (m_Input == null)
				{
					m_Input = GetComponent<FirstPersonInput>();
				}

				return m_Input;
			}
		}

		/// <summary>
		/// Get the attached implementations on awake
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			m_MovementEffects.Init(this);
			m_CurrentMovementProperties = m_Walking;
			m_NewMovementProperties = m_Walking;
			m_MovementEffects.AdjustTriggerThreshold(m_CurrentMovementProperties.strideLength);
			m_MainCamera = Camera.main;
		}

		// Subscribes to the various events
		protected override void OnEnable()
		{
			base.OnEnable();
			controllerAdapter.landed += OnLanded;
			m_MovementEffects.Subscribe();
			characterInput.jumpPressed += OnJumpPressed;
			characterInput.sprintStarted += StartSprinting;
			characterInput.sprintEnded += StartWalking;
			characterInput.crouchStarted += StartCrouching;
			characterInput.crouchEnded += StartWalking;
		}

		// Unsubscribes to the various events
		protected override void OnDisable()
		{
			base.OnDisable();
			controllerAdapter.landed -= OnLanded;
			m_MovementEffects.Unsubscribe();
			if (characterInput == null)
			{
				return;
			}

			characterInput.jumpPressed -= OnJumpPressed;
			characterInput.sprintStarted -= StartSprinting;
			characterInput.sprintEnded -= StartWalking;
			characterInput.crouchStarted -= StartCrouching;
			characterInput.crouchEnded -= StartWalking;
		}

		// Handles movement and rotation
		void FixedUpdate()
		{
			var currentRotation = transform.rotation.eulerAngles;			
			currentRotation.y = m_MainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
			Move();
			m_MovementEffects.Tick();
		}

		/// <summary>
		/// Change state to the new state and adds to previous state stack
		/// </summary>
		/// <param name="newState">The new first person movement properties to be used</param>
		public void EnterNewState(MovementProperties newState)
		{
			ChangeState(newState);
		}

		/// <summary>
		/// Resets state to previous state
		/// </summary>
		public void ResetState()
		{
			ChangeState(m_Walking);
		}		

		// Called on character landing
		void OnLanded()
		{
			m_CurrentMovementProperties = m_NewMovementProperties;
		}

		// Handles jumping
		void OnJumpPressed()
		{
			if (controllerAdapter.isGrounded && m_CurrentMovementProperties.canJump)
			{
				controllerAdapter.SetJumpVelocity(m_CurrentMovementProperties.jumpSpeed);
			}
		}

		// State based movement
		void Move()
		{
			if (!characterInput.hasMovementInput)
			{
				m_CurrentSpeed = 0f;
			}

			var move
				= characterInput.moveInput;
			if (move.sqrMagnitude > 1)
			{
				move.Normalize();
			}

			var forward = transform.forward * move.y;
			var sideways = transform.right * move.x;
			var currentVelocity = (forward + sideways) * m_CurrentMovementProperties.maxSpeed;
			m_CurrentSpeed = currentVelocity.magnitude;
			controllerAdapter.Move(currentVelocity * Time.fixedDeltaTime, Time.fixedDeltaTime);
		}

		// Sets the character to the walking state
		void StartWalking()
		{
			characterInput.ResetInputs();
			ChangeState(m_Walking);
			if (startWalking != null)
			{
				startWalking();
			}
		}

		// Sets the character to the sprinting state
		void StartSprinting()
		{
			ChangeState(m_Sprinting);
			if (startSprinting != null)
			{
				startSprinting();
			}
		}

		// Sets the character to crouching state
		void StartCrouching()
		{
			ChangeState(m_Crouching);
			if (startCrouching != null)
			{
				startCrouching();
			}
		}

		// Changes the current motor state and play events associated with state change
		void ChangeState(MovementProperties newState)
		{
			m_NewMovementProperties = newState;

			if (controllerAdapter.isGrounded)
			{
				m_CurrentMovementProperties = m_NewMovementProperties;
			}

			m_MovementEffects.AdjustTriggerThreshold(newState.strideLength);
		}
		
		/// <summary>
		/// Can moving platforms rotate the current camera?
		/// </summary>
		public override bool MovingPlatformCanRotateCamera()
		{
			return true;
		}
	}
	
	/// <summary>
	/// Handles movement events for First person character
	/// </summary>
	[Serializable]
	public class FirstPersonMovementEventHandler : MovementEventHandler
	{
		[SerializeField, Tooltip("The maximum speed of the character")]
		float m_MaximumSpeed = 10f;
		
		[SerializeField, Tooltip("Layermask used for checking movement area")]
		LayerMask m_LayerMask;

		// Squared travel distance of the character before being reset by threshold
		float m_SqrTravelledDistance;

		// Squared travel distance threshold of the character - based on stride length
		float m_SqrDistanceThreshold;

		// Position of the character on previous frame
		Vector3 m_PreviousPosition;

		// Is the character currently striding with the left foot forward
		bool m_IsLeftFoot;

		// Cached transform of the character brain
		Transform m_Transform;

		/// <summary>
		/// Initializes the handler with the correct character brain and sets up the transform and previousPosition needed to calculate distance travelled
		/// </summary>
		/// <param name="brainToUse"></param>
		public void Init(FirstPersonBrain brainToUse)
		{
			base.Init(brainToUse);
			m_Transform = brainToUse.transform;
			m_PreviousPosition = m_Transform.position;
		}

		/// <summary>
		/// Updates the distance travelled and checks if footstep events need to be fired
		/// </summary>
		public void Tick()
		{
			var currentPosition = m_Transform.position;

			//If the character has not moved or is not grounded then ignore the calculations that follow
			if (currentPosition == m_PreviousPosition || !brain.controllerAdapter.isGrounded)
			{
				m_PreviousPosition = currentPosition;
				return;
			}

			m_SqrTravelledDistance += (currentPosition - m_PreviousPosition).sqrMagnitude;

			if (m_SqrTravelledDistance >= m_SqrDistanceThreshold)
			{
				m_SqrTravelledDistance = 0;
				var data =
					new MovementEventData(m_Transform, Mathf.Clamp01(brain.planarSpeed / m_MaximumSpeed));
				PlayFootstep(data);

				m_IsLeftFoot = !m_IsLeftFoot;
			}

			m_PreviousPosition = currentPosition;
		}

		/// <summary>
		/// Subscribes to jump and land events on the brain's controller adapter
		/// </summary>
		public void Subscribe()
		{
			if (brain == null || brain.controllerAdapter == null)
			{
				return;
			}
			brain.controllerAdapter.landed += Landed;
			brain.controllerAdapter.jumpVelocitySet += Jumped;
		}

		/// <summary>
		/// Unsubscribes to jump and land events on the brain's controller adapter
		/// </summary>
		public void Unsubscribe()
		{
			if (brain == null || brain.controllerAdapter == null)
			{
				return;
			}
			brain.controllerAdapter.landed -= Landed;
			brain.controllerAdapter.jumpVelocitySet -= Jumped;
		}

		/// <summary>
		/// Change the distance that footstep sounds are played
		/// </summary>
		/// <param name="strideLength"></param>
		public void AdjustTriggerThreshold(float strideLength)
		{
			m_SqrDistanceThreshold = strideLength * strideLength;
		}
		
		// Calls PlayEvent on the jump ID
		void Jumped()
		{
			CheckArea();
			PlayJumping(new MovementEventData(m_Transform));
		}

		// Calls PlayEvent on the landing ID
		void Landed()
		{
			CheckArea();
			PlayLanding(new MovementEventData(m_Transform));
		}
		
		// Handles footstep effects
		void PlayFootstep(MovementEventData movementEventData)
		{
			CheckArea();
			if (m_IsLeftFoot)
			{
				PlayLeftFoot(movementEventData);
			}
			else
			{
				PlayRightFoot(movementEventData);
			}
			
			m_IsLeftFoot = !m_IsLeftFoot;
		}

		// Checks PhysicMaterial where the character is standing
		void CheckArea()
		{
			RaycastHit hit;
			if (UnityEngine.Physics.Raycast(m_Transform.position, Vector3.down, out hit, m_LayerMask))
			{
				SetPhysicMaterial(hit.collider.sharedMaterial);
			}
		}
	}
}
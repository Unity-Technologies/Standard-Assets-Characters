using System;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// The main controller of first person character
	/// Ties together the input and physics implementations
	/// </summary>
	[RequireComponent(typeof(FirstPersonInput))]
	public class FirstPersonBrain : CharacterBrain
	{
		/// <summary>
		/// The names of the states in the camera animator
		/// </summary>
		const string k_CrouchAnimationStateName = "Crouch",
		                     k_SprintAnimationStateName = "Sprint",
		                     k_WalkAnimationStateName = "Walk";

		/// <summary>
		/// Stores movement properties for the different states - e.g. walk
		/// </summary>
		[Serializable]
		public struct MovementProperties
		{
			/// <summary>
			/// The maximum movement speed
			/// </summary>
			[FormerlySerializedAs("maxSpeed")]
			[SerializeField, Tooltip("The maximum movement speed of the character"), Range(0.1f, 20f)]
			float m_MaxSpeed;

			/// <summary>
			/// The initial Y velocity of a Jump
			/// </summary>
			[FormerlySerializedAs("jumpSpeed")]
			[SerializeField, Tooltip("The initial Y velocity of a Jump"), Range(0f, 10f)]
			float m_JumpSpeed;

			/// <summary>
			/// The length of a stride
			/// </summary>
			[FormerlySerializedAs("strideLength")]
			[SerializeField, Tooltip("Distance that is considered a stride"), Range(0f, 1f)]
			float m_StrideLength;

			/// <summary>
			/// Can the first person character jump in this state
			/// </summary>
			public bool canJump
			{
				get { return m_JumpSpeed > 0f; }
			}

			public float maxSpeed
			{
				get { return m_MaxSpeed; }
			}

			public float jumpSpeed
			{
				get { return m_JumpSpeed; }
			}

			public float strideLength
			{
				get { return m_StrideLength; }
			}
		}

		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[FormerlySerializedAs("walking")]
		[SerializeField, Tooltip("Movement properties of the character while walking")]
		MovementProperties m_Walking;

		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[FormerlySerializedAs("sprinting")]
		[SerializeField, Tooltip("Movement properties of the character while sprinting")]
		MovementProperties m_Sprinting;

		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[FormerlySerializedAs("crouching")]
		[SerializeField, Tooltip("Movement properties of the character while crouching")]
		protected MovementProperties m_Crouching;

		/// <summary>
		/// Manages movement events
		/// </summary>
		[FormerlySerializedAs("firstPersonMovementEventHandler")]
		[SerializeField, Tooltip("The management of movement events e.g. footsteps")]
		FirstPersonMovementEventHandler m_FirstPersonMovementEventHandler;

		/// <summary>
		/// The movement state is passed to the camera manager so that there can be different cameras e.g. crouch
		/// </summary>
		FirstPersonCameraController m_FirstPersonCameraController;

		/// <summary>
		/// The current movement properties
		/// </summary>
		float m_CurrentSpeed;

		/// <summary>
		/// Backing field to prevent the currentProperties from being null
		/// </summary>
		MovementProperties m_CurrentMovementProperties;

		/// <summary>
		/// Stores the new movement properties if the character tries to change movement state in mid-air
		/// </summary>
		MovementProperties m_NewMovementProperties;
		
		/// <summary>
		/// Cached instance of Camera.main
		/// </summary>
		Camera m_MainCamera;

		/// <summary>
		/// Backing field for lazily loading the character input
		/// </summary>
		FirstPersonInput m_Input;

		/// <summary>
		/// Lazily loads the <see cref="FirstPersonInput"/>
		/// </summary>
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

		/// <summary>
		/// Helper method for setting the animation
		/// </summary>
		/// <param name="animation">The case sensitive name of the animation state</param>
		void SetAnimation(string animation)
		{
			if (m_FirstPersonCameraController == null)
			{
				Debug.LogWarning("No camera animation manager setup");
				return;
			}

			m_FirstPersonCameraController.SetAnimation(animation);
		}

		/// <summary>
		/// Get the attached implementations on awake
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			FindCameraController(true);
			m_FirstPersonMovementEventHandler.Init(this);
			m_CurrentMovementProperties = m_Walking;
			m_NewMovementProperties = m_Walking;
			m_FirstPersonMovementEventHandler.AdjustTriggerThreshold(m_CurrentMovementProperties.strideLength);
			m_MainCamera = Camera.main;
		}

		/// <summary>
		/// Checks if the <see cref="FirstPersonCameraController"/> has been assigned otherwise finds it in the scene
		/// </summary>
		void FindCameraController(bool autoDisable)
		{
			if (m_FirstPersonCameraController == null)
			{
				var firstPersonCameraControllers =
					FindObjectsOfType<FirstPersonCameraController>();

				var length = firstPersonCameraControllers.Length;
				if (length != 1)
				{
					var errorMessage = "No FirstPersonCameraAnimationManagers in scene! Disabling Brain";
					if (length > 1)
					{
						errorMessage = "Too many FirstPersonCameraAnimationManagers in scene! Disabling Brain";
					}

					if (autoDisable)
					{
						Debug.LogError(errorMessage);
						gameObject.SetActive(false);	
					}
					
					return;
				}

				m_FirstPersonCameraController = firstPersonCameraControllers[0];
			}

			m_FirstPersonCameraController.SetupBrain(this);
		}

		/// <summary>
		/// Subscribes to the various events
		/// </summary>
		void OnEnable()
		{
			m_FirstPersonMovementEventHandler.Subscribe();
			characterInput.jumpPressed += OnJumpPressed;
			characterInput.sprintStarted += StartSprinting;
			characterInput.sprintEnded += StartWalking;
			characterInput.crouchStarted += StartCrouching;
			characterInput.crouchEnded += StartWalking;
			controllerAdapter.landed += OnLanded;
		}

		/// <summary>
		/// Unsubscribes to the various events
		/// </summary>
		void OnDisable()
		{
			m_FirstPersonMovementEventHandler.Unsubscribe();
			if (characterInput == null)
			{
				return;
			}

			characterInput.jumpPressed -= OnJumpPressed;
			characterInput.sprintStarted -= StartSprinting;
			characterInput.sprintEnded -= StartWalking;
			characterInput.crouchStarted -= StartCrouching;
			characterInput.crouchEnded -= StartWalking;
			controllerAdapter.landed -= OnLanded;
		}

		/// <summary>
		/// Called on character landing
		/// </summary>
		void OnLanded()
		{
			m_CurrentMovementProperties = m_NewMovementProperties;
		}

		/// <summary>
		/// Handles jumping
		/// </summary>
		void OnJumpPressed()
		{
			if (controllerAdapter.isGrounded && m_CurrentMovementProperties.canJump)
			{
				controllerAdapter.SetJumpVelocity(m_CurrentMovementProperties.jumpSpeed);
			}
		}

		/// <summary>
		/// Handles movement and rotation
		/// </summary>
		void FixedUpdate()
		{
			var currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = m_MainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
			Move();
			m_FirstPersonMovementEventHandler.Tick();
		}

		/// <summary>
		/// State based movement
		/// </summary>
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

		/// <summary>
		/// Sets the character to the walking state
		/// </summary>
		void StartWalking()
		{
			characterInput.ResetInputs();
			ChangeState(m_Walking);
			SetAnimation(k_WalkAnimationStateName);
		}

		/// <summary>
		/// Sets the character to the sprinting state
		/// </summary>
		void StartSprinting()
		{
			ChangeState(m_Sprinting);
			SetAnimation(k_SprintAnimationStateName);
		}

		/// <summary>
		/// Sets the character to crouching state
		/// </summary>
		void StartCrouching()
		{
			ChangeState(m_Crouching);
			SetAnimation(k_CrouchAnimationStateName);
		}

		/// <summary>
		/// Changes the current motor state and play events associated with state change
		/// </summary>
		/// <param name="newState"></param>
		void ChangeState(MovementProperties newState)
		{
			m_NewMovementProperties = newState;

			if (controllerAdapter.isGrounded)
			{
				m_CurrentMovementProperties = m_NewMovementProperties;
			}

			m_FirstPersonMovementEventHandler.AdjustTriggerThreshold(newState.strideLength);
		}
		
#if UNITY_EDITOR
		void OnValidate()
		{
			//Design pattern for fetching required scene references
			FindCameraController(false);
		}

		void Reset()
		{
			//Design pattern for fetching required scene references
			FindCameraController(false);
		}
#endif

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
	}
	
	/// <summary>
	/// Handles movement events for First person character
	/// </summary>
	[Serializable]
	public class FirstPersonMovementEventHandler : MovementEventHandler
	{
		[FormerlySerializedAs("maximumSpeed")]
		[SerializeField, Tooltip("The maximum speed of the character")]
		protected float m_MaximumSpeed = 10f;

		float m_SqrTravelledDistance;

		float m_SqrDistanceThreshold;

		Vector3 m_PreviousPosition;

		bool m_IsLeftFoot;

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
				if (m_IsLeftFoot)
				{
					PlayLeftFoot(data);
				}
				else
				{
					PlayRightFoot(data);
				}

				m_IsLeftFoot = !m_IsLeftFoot;
			}

			m_PreviousPosition = currentPosition;
		}

		/// <summary>
		/// Subscribe
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
		/// Unsubscribe
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
		
		/// <summary>
		/// Calls PlayEvent on the jump ID
		/// </summary>
		void Jumped()
		{
			PlayJumping(new MovementEventData(m_Transform));
		}

		/// <summary>
		/// Calls PlayEvent on the landing ID
		/// </summary>
		void Landed()
		{
			PlayLanding(new MovementEventData(m_Transform));
		}
	}
}
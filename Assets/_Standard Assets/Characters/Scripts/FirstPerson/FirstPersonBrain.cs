using System;
using Cinemachine;
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
	[RequireComponent(typeof(CharacterPhysics))]
	[RequireComponent(typeof(FirstPersonInput))]
	public class FirstPersonBrain : CharacterBrain
	{
		/// <summary>
		/// The names of the states in the camera animator
		/// </summary>
		private const string k_CrouchAnimationStateName = "Crouch",
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
			[SerializeField, Tooltip("The maximum movement speed of the character"), Range(0.1f, 20f)]
			public float maxSpeed;

			/// <summary>
			/// The initial Y velocity of a Jump
			/// </summary>
			[SerializeField, Tooltip("The initial Y velocity of a Jump"), Range(0f, 10f)]
			public float jumpSpeed;

			/// <summary>
			/// The length of a stride
			/// </summary>
			[SerializeField, Tooltip("Distance that is considered a stride"), Range(0f, 1f)]
			public float strideLength;

			/// <summary>
			/// Can the first person character jump in this state
			/// </summary>
			public bool canJump
			{
				get { return jumpSpeed > 0f; }
			}
		}

		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[SerializeField, Tooltip("Movement properties of the character while walking")]
		protected MovementProperties walking;

		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[SerializeField, Tooltip("Movement properties of the character while sprinting")]
		protected MovementProperties sprinting;

		/// <summary>
		/// The state that first person motor starts in
		/// </summary>
		[SerializeField, Tooltip("Movement properties of the character while crouching")]
		protected MovementProperties crouching;

		/// <summary>
		/// Main Camera that is using the POV camera
		/// </summary>
		[SerializeField,
		 Tooltip("Main Camera that is using the POV camera - will fetch Camera.main if this is left empty")]
		protected Camera mainCamera;

		/// <summary>
		/// Manages movement events
		/// </summary>
		[SerializeField, Tooltip("The management of movement events e.g. footsteps")]
		protected FirstPersonMovementEventHandler firstPersonMovementEventHandler;

		/// <summary>
		/// The movement state is passed to the camera manager so that there can be different cameras e.g. crouch
		/// </summary>
		[SerializeField,
		 Tooltip(
			 "The movement state is passed to the camera manager so that there can be different cameras e.g. crouch")]
		protected FirstPersonCameraController firstPersonCameraController;

		/// <summary>
		/// The current movement properties
		/// </summary>
		private float currentSpeed;

		/// <summary>
		/// Backing field to prevent the currentProperties from being null
		/// </summary>
		private MovementProperties currentMovementProperties;

		/// <summary>
		/// Stores the new movement properties if the character tries to change movement state in mid-air
		/// </summary>
		private MovementProperties newMovementProperties;

		/// <summary>
		/// Backing field for lazily loading the character input
		/// </summary>
		private FirstPersonInput input;

		/// <summary>
		/// Lazily loads the <see cref="FirstPersonInput"/>
		/// </summary>
		private FirstPersonInput characterInput
		{
			get
			{
				if (input == null)
				{
					input = GetComponent<FirstPersonInput>();
				}

				return input;
			}
		}

		/// <summary>
		/// Gets the referenced <see cref="CameraController"/>
		/// </summary>
		public CameraController cameraController
		{
			get { return firstPersonCameraController; }
		}

		/// <summary>
		/// Returns the characters normalized speed
		/// </summary>
		public override float normalizedForwardSpeed
		{
			get
			{
				float maxSpeed = currentMovementProperties.maxSpeed;
				if (maxSpeed <= 0)
				{
					return 1;
				}

				return currentSpeed / maxSpeed;
			}
		}

		/// <summary>
		/// Gets the MovementEventHandler
		/// </summary>
		public override MovementEventHandler movementEventHandler
		{
			get { return firstPersonMovementEventHandler; }
		}

		/// <summary>
		/// Gets the target Y rotation of the character
		/// </summary>
		public override float targetYRotation { get; set; }

		/// <summary>
		/// Helper method for setting the animation
		/// </summary>
		/// <param name="animation">The case sensitive name of the animation state</param>
		private void SetAnimation(string animation)
		{
			if (firstPersonCameraController == null)
			{
				Debug.LogWarning("No camera animation manager setup");
				return;
			}

			firstPersonCameraController.SetAnimation(animation);
		}

		/// <summary>
		/// Get the attached implementations on awake
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			CheckCameraAnimationManager();
			firstPersonMovementEventHandler.Init(this);
			currentMovementProperties = walking;
			newMovementProperties = walking;
			firstPersonMovementEventHandler.AdjustTriggerThreshold(currentMovementProperties.strideLength);

			if (mainCamera == null)
			{
				mainCamera = Camera.main;
			}
		}

		/// <summary>
		/// Checks if the <see cref="FirstPersonCameraController"/> has been assigned otherwise finds it in the scene
		/// </summary>
		private void CheckCameraAnimationManager()
		{
			if (firstPersonCameraController == null)
			{
				Debug.LogWarning("Camera Animation Manager not set - looking in scene");
				FirstPersonCameraController[] firstPersonCameraControllers =
					FindObjectsOfType<FirstPersonCameraController>();

				int length = firstPersonCameraControllers.Length;
				if (length != 1)
				{
					string errorMessage = "No FirstPersonCameraAnimationManagers in scene! Disabling Brain";
					if (length > 1)
					{
						errorMessage = "Too many FirstPersonCameraAnimationManagers in scene! Disabling Brain";
					}

					Debug.LogError(errorMessage);
					gameObject.SetActive(false);
					return;
				}

				firstPersonCameraController = firstPersonCameraControllers[0];
			}

			firstPersonCameraController.SetupBrain(this);
		}

		/// <summary>
		/// Subscribes to the various events
		/// </summary>
		private void OnEnable()
		{
			firstPersonMovementEventHandler.Subscribe();
			characterInput.jumpPressed += OnJumpPressed;
			characterInput.sprintStarted += StartSprinting;
			characterInput.sprintEnded += StartWalking;
			characterInput.crouchStarted += StartCrouching;
			characterInput.crouchEnded += StartWalking;
			characterPhysics.landed += OnLanded;
		}

		/// <summary>
		/// Unsubscribes to the various events
		/// </summary>
		private void OnDisable()
		{
			firstPersonMovementEventHandler.Unsubscribe();
			if (characterInput == null)
			{
				return;
			}

			characterInput.jumpPressed -= OnJumpPressed;
			characterInput.sprintStarted -= StartSprinting;
			characterInput.sprintEnded -= StartWalking;
			characterInput.crouchStarted -= StartCrouching;
			characterInput.crouchEnded -= StartWalking;
			characterPhysics.landed -= OnLanded;
		}

		/// <summary>
		/// Called on character landing
		/// </summary>
		private void OnLanded()
		{
			currentMovementProperties = newMovementProperties;
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
		/// Handles movement and rotation
		/// </summary>
		private void FixedUpdate()
		{
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = mainCamera.transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);
			Move();
			firstPersonMovementEventHandler.Tick();
		}

		/// <summary>
		/// State based movement
		/// </summary>
		private void Move()
		{
			if (!characterInput.hasMovementInput)
			{
				currentSpeed = 0f;
			}

			Vector2 move
				= characterInput.moveInput;
			if (move.sqrMagnitude > 1)
			{
				move.Normalize();
			}

			Vector3 forward = transform.forward * move.y;
			Vector3 sideways = transform.right * move.x;
			Vector3 currentVelocity = (forward + sideways) * currentMovementProperties.maxSpeed;
			currentSpeed = currentVelocity.magnitude;
			characterPhysics.Move(currentVelocity * Time.fixedDeltaTime, Time.fixedDeltaTime);
		}

		/// <summary>
		/// Sets the character to the walking state
		/// </summary>
		private void StartWalking()
		{
			characterInput.ResetInputs();
			ChangeState(walking);
			SetAnimation(k_WalkAnimationStateName);
		}

		/// <summary>
		/// Sets the character to the sprinting state
		/// </summary>
		private void StartSprinting()
		{
			ChangeState(sprinting);
			SetAnimation(k_SprintAnimationStateName);
		}

		/// <summary>
		/// Sets the character to crouching state
		/// </summary>
		private void StartCrouching()
		{
			ChangeState(crouching);
			SetAnimation(k_CrouchAnimationStateName);
		}

		/// <summary>
		/// Changes the current motor state and play events associated with state change
		/// </summary>
		/// <param name="newState"></param>
		private void ChangeState(MovementProperties newState)
		{
			newMovementProperties = newState;

			if (characterPhysics.isGrounded)
			{
				currentMovementProperties = newMovementProperties;
			}

			firstPersonMovementEventHandler.AdjustTriggerThreshold(newState.strideLength);
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
			ChangeState(walking);
		}
	}
	
	/// <summary>
	/// Handles movement events for First person character
	/// </summary>
	[Serializable]
	public class FirstPersonMovementEventHandler : DistanceMovementEventHandler
	{
		/// <summary>
		/// Sets the brain to be used
		/// </summary>
		public void Init(FirstPersonBrain brainToUse)
		{
			base.Init(brainToUse);
		}

		/// <summary>
		/// Change the distance that footstep sounds are played
		/// </summary>
		/// <param name="strideLength"></param>
		public void AdjustTriggerThreshold(float strideLength)
		{
			sqrDistanceThreshold = strideLength * strideLength;
		}
	}
}
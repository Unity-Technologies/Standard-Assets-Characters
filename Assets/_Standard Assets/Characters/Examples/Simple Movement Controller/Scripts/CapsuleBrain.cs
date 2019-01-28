using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	/// <summary>
	/// Simple third person character without animation which uses the <see cref="StandardAssets.Characters.Physics.OpenCharacterController"/>
	/// </summary>
	public class CapsuleBrain : CharacterBrain
	{
		[Header("Capsule Brain")]
		[SerializeField, Tooltip("Character's max movement speed")]
		float m_MaxSpeed = 5f;

		[SerializeField, Tooltip("Time take to accelerate from rest to max speed")]
		float m_TimeToMaxSpeed = 0.5f;
		
		[SerializeField, Tooltip("Character's rotational speed")] 
		float m_TurnSpeed = 300f;

		[SerializeField, Tooltip("Initial upward velocity applied on jumping")]
		float m_JumpSpeed = 5f;
	   
		// The current movement properties
		float m_CurrentSpeed;

		// The current movement properties
		float m_MovementTime;

		// A check to see if input was previous being applied
		bool m_PreviouslyHasInput;

		// The main camera's transform, used for calculating look direction.
		Transform m_MainCameraTransform;

		// Reference to input system
		CapsuleInput m_Input;

		// Gets the character's input - lazy load mechanism
		CapsuleInput characterInput
		{
			get
			{
				if (m_Input == null)
				{
					m_Input = GetComponent<CapsuleInput>();
				}

				return m_Input;
			}
		}

		/// <summary>
		/// Gets the character's current movement speed divided by maximum movement speed
		/// </summary>
		public override float normalizedForwardSpeed
		{
			get
			{
				return m_CurrentSpeed / m_MaxSpeed;
			}
		}

		/// <summary>
		/// Gets the character's target Y rotation
		/// </summary>
		public override float targetYRotation { get; set; }

		/// <summary>
		/// Caches main camera
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			m_MainCameraTransform = Camera.main.transform;
		}

		/// <summary>
		/// Subscribes to jump input
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			characterInput.jumpPressed += OnJumpPressed;
		}
		
		// Unsubscribe from jump input
		protected override void OnDisable()
		{
			base.OnDisable();
			if (characterInput == null)
			{
				return;
			}
			
			characterInput.jumpPressed -= OnJumpPressed;
		}
		
		// Handles camera rotation
		protected override void Update()
		{
			base.Update();
			if (!characterInput.hasMovementInput)
			{
				return;
			}
			var targetRotation = CalculateTargetRotation();
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_TurnSpeed * Time.deltaTime);

			targetYRotation = targetRotation.eulerAngles.y;
		}
		
		// Calculates the character's target rotation based on input and the camera rotation
		// returns Character's target rotation
		Quaternion CalculateTargetRotation()
		{
			var flatForward = m_MainCameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			var localMovementDirection = new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);
			var cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		// Handles jumping
		void OnJumpPressed()
		{
			if (controllerAdapter.isGrounded)
			{
				controllerAdapter.SetJumpVelocity(m_JumpSpeed);
			}	
		}

		// Handles movement on Physics update
		void FixedUpdate()
		{
			Move();
		}

		// State based movement
		void Move()
		{
			if (characterInput.hasMovementInput)
			{
				if (!m_PreviouslyHasInput)
				{
					m_MovementTime = 0f;
				}
				Accelerate();
			}
			else
			{
				if (m_PreviouslyHasInput)
				{
					m_MovementTime = 0f;
				}

				Stop();
			}

			var input = characterInput.moveInput;
			if (input.sqrMagnitude > 1)
			{
				input.Normalize();
			}
		
			var forward = transform.forward * input.magnitude;
			var sideways = Vector3.zero;
			
			controllerAdapter.Move((forward + sideways) * m_CurrentSpeed * Time.fixedDeltaTime, Time.fixedDeltaTime);

			m_PreviouslyHasInput = characterInput.hasMovementInput;
		}	

		// Calculates current speed based on acceleration anim curve
		void Accelerate()
		{
			m_MovementTime += Time.fixedDeltaTime;
			m_MovementTime = Mathf.Clamp(m_MovementTime, 0f, m_TimeToMaxSpeed);
			m_CurrentSpeed = m_MovementTime / m_TimeToMaxSpeed * m_MaxSpeed;
		}
		
		// Stops the movement
		void Stop()
		{
			m_CurrentSpeed = 0f;
		}
		
		/// <summary>
		/// Can moving platforms rotate the current camera?
		/// </summary>
		public override bool MovingPlatformCanRotateCamera()
		{
			return false;
		}
	}
}
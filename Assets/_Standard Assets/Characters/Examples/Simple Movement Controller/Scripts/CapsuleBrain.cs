using StandardAssets.Characters.Common;
using UnityEngine;
using UnityEngine.Serialization;

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
	   
		/// <summary>
		/// The current movement properties
		/// </summary>
		float m_CurrentSpeed;

		/// <summary>
		/// The current movement properties
		/// </summary>
		float m_MovementTime;

		/// <summary>
		/// A check to see if input was previous being applied
		/// </summary>
		bool m_PreviouslyHasInput;

		/// <summary>
		/// The main camera's transform, used for calculating look direction.
		/// </summary>
		Transform m_MainCameraTransform;

		/// <summary>
		/// Reference to input system
		/// </summary>
		CapsuleInput m_Input;

		/// <summary>
		/// Gets the character's input - lazy load mechanism
		/// </summary>
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
		void OnEnable()
		{
			characterInput.jumpPressed += OnJumpPressed;
		}
		
		/// <summary>
		/// Unsubscribe from jump input
		/// </summary>
		void OnDisable()
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
		
		/// <summary>
		/// Calculates the character's target rotation based on input and the camera rotation
		/// </summary>
		/// <returns>Character's target rotation</returns>
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

		/// <summary>
		/// Handles jumping
		/// </summary>
		void OnJumpPressed()
		{
			if (controllerAdapter.isGrounded)
			{
				controllerAdapter.SetJumpVelocity(m_JumpSpeed);
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

		/// <summary>
		/// Calculates current speed based on acceleration anim curve
		/// </summary>
		void Accelerate()
		{
			m_MovementTime += Time.fixedDeltaTime;
			m_MovementTime = Mathf.Clamp(m_MovementTime, 0f, m_TimeToMaxSpeed);
			m_CurrentSpeed = m_MovementTime / m_TimeToMaxSpeed * m_MaxSpeed;
		}
		
		/// <summary>
		/// Stops the movement
		/// </summary>
		void Stop()
		{
			m_CurrentSpeed = 0f;
		}
	}
}
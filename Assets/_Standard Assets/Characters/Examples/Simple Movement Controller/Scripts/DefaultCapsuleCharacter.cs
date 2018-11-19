using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	/// <summary>
	/// Implementation of an animation free third person character that uses Unity's CharacterController
	/// </summary>
	[RequireComponent(typeof(CapsuleInput))]
	[RequireComponent(typeof(CharacterController))]
	public class DefaultCapsuleCharacter : MonoBehaviour
	{
		[SerializeField, Tooltip("Character's maximum movement speed")]
		float m_MaxSpeed = 5f;

		[SerializeField, Tooltip("Time take to accelerate from rest to max speed")]
		float m_TimeToMaxSpeed = 0.5f;

		[SerializeField, Tooltip("Character's rotational speed")]
		float m_TurnSpeed = 300f;
		
		[SerializeField, Tooltip("Downward distance from transform centre to check if character is grounded")]
		float m_GroundCheckDistance = 0.51f;

		[SerializeField, Tooltip("Layer mask to check for grounding via ray cast")]
		LayerMask m_GroundCheckMask;

		[SerializeField, Tooltip("Acceleration due to gravity")]
		float m_Gravity = -9.81f;

		[SerializeField, Tooltip("Maximum falling speed of the character")]
		float m_TerminalVelocity = -100f;

		[SerializeField, Tooltip("Initial upward velocity applied on jumping")]
		float m_JumpSpeed = 10f;

		// Counter of the current movement time.
		float m_MovementTime;
		
		// Current movement speed.
		float m_CurrentSpeed;
		
		// Time character has been in an aerial state.
		float m_AirTime;

		// Last applied vertical velocity.
		float m_CurrentVerticalVelocity;

		// Velocity applied as the jump was initiated.
		float m_InitialJumpVelocity;
		
		// Time character has been in a fall state.
		float m_FallTime;

		// Whether there was input last fixed update.
		bool m_PreviouslyHasInput;

		// Cached reference of the CapsuleInput.
		CapsuleInput m_CharacterInput;

		// Cached reference of the CharacterController.
		CharacterController m_Controller;

		// Cached reference of the main camera.
		Transform m_MainCameraTransform;

		// Stores the m_CurrentVerticalVelocity as the y component.
		Vector3 m_VerticalVector;

		// Cache main camera and get required components
		void Awake()
		{
			m_CharacterInput = GetComponent<CapsuleInput>();
			m_Controller = GetComponent<CharacterController>();
			m_MainCameraTransform = Camera.main.transform;
		}

		// Subscribe to jump input
		void OnEnable()
		{
			m_CharacterInput.jumpPressed += OnJump;
		}

		// Set the initial jump velocity
		void OnJump()
		{
			m_InitialJumpVelocity = m_JumpSpeed;
		}

		// Unsubscribe from jump input
		void OnDisable()
		{
			m_CharacterInput.jumpPressed -= OnJump;
		}

		// Calculate character movement and look angle
		void Update()
		{
			if (!m_CharacterInput.hasMovementInput)
			{
				return;
			}

			var flatForward = m_MainCameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			var localMovementDirection = new Vector3(m_CharacterInput.moveInput.x, 0f, m_CharacterInput.moveInput.y);
			var cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			var targetRotation = Quaternion.LookRotation(cameraToInputOffset * flatForward);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_TurnSpeed * Time.deltaTime);
		}
		
		// Handles movement on Physics update
		void FixedUpdate()
		{
			AerialMovement();
			if (m_CharacterInput.hasMovementInput)
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

			var input = m_CharacterInput.moveInput;
			if (input.sqrMagnitude > 1)
			{
				input.Normalize();
			}
		
			var forward = transform.forward * input.magnitude;
			var sideways = Vector3.zero;
			
			m_Controller.Move(((forward + sideways) * m_CurrentSpeed * Time.fixedDeltaTime) + m_VerticalVector);

			m_PreviouslyHasInput = m_CharacterInput.hasMovementInput;
		}

		// Calculates current speed based on acceleration anim curve
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

		// Checks if the character is grounded
		// returns true if character is grounded
		bool CheckGrounded()
		{
			Debug.DrawRay(transform.position + m_Controller.center, 
			              new Vector3(0,-m_GroundCheckDistance * m_Controller.height,0), Color.red);
			if (UnityEngine.Physics.Raycast(transform.position + m_Controller.center, 
			                                -transform.up, m_GroundCheckDistance * m_Controller.height, m_GroundCheckMask))
			{
				return true;
			}
			
			var xRayOffset = new Vector3(m_Controller.radius,0f,0f);
			var zRayOffset = new Vector3(0f,0f,m_Controller.radius);		
			
			for (var i = 0; i < 4; i++)
			{
				var sign = 1f;
				Vector3 rayOffset;
				if (i % 2 == 0)
				{
					rayOffset = xRayOffset;
					sign = i - 1f;
				}
				else
				{
					rayOffset = zRayOffset;
					sign = i - 2f;
				}
				Debug.DrawRay(transform.position + m_Controller.center + sign * rayOffset, 
				              new Vector3(0,-m_GroundCheckDistance * m_Controller.height,0), Color.blue);

				if (UnityEngine.Physics.Raycast(transform.position + m_Controller.center + sign * rayOffset,
				                                -transform.up,m_GroundCheckDistance * m_Controller.height, m_GroundCheckMask))
				{
					return true;
				}
			}
			return false;
		}

		// Handles that character's vertical velocity (jumping and falling)
		void AerialMovement()
		{
			m_AirTime += Time.fixedDeltaTime;
			if (m_CurrentVerticalVelocity >= 0.0f)
			{
				m_CurrentVerticalVelocity = Mathf.Clamp(m_InitialJumpVelocity + m_Gravity * m_AirTime, m_TerminalVelocity,
				                                      Mathf.Infinity);
			}

			if (m_CurrentVerticalVelocity < 0.0f)
			{
				m_CurrentVerticalVelocity = Mathf.Clamp(m_Gravity * m_FallTime, m_TerminalVelocity, Mathf.Infinity);
				m_FallTime += Time.fixedDeltaTime;
				if (CheckGrounded())
				{
					m_InitialJumpVelocity = 0.0f;
					m_VerticalVector = Vector3.zero;

					m_FallTime = 0.0f;
					m_AirTime = 0.0f;
					return;
				}
			}

			m_VerticalVector = new Vector3(0.0f, m_CurrentVerticalVelocity * Time.fixedDeltaTime, 0.0f);
		}
	}
}
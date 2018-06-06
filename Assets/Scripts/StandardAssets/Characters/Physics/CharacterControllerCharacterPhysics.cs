using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A physic implementation that uses the default Unity character controller
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class CharacterControllerCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{
		/// <summary>
		/// The value of gravity
		/// </summary>
		public float gravity;

		/// <summary>
		/// The maximum speed that the character can move downwards
		/// </summary>
		public float terminalVelocity = 10f;

		/// <summary>
		/// The distance used to check if grounded
		/// </summary>
		public float groundCheckDistance = 0.51f;

		/// <summary>
		/// Layers to use in the ground check
		/// </summary>
		[Tooltip("Layers to use in the ground check")]
		public LayerMask groundCheckMask;

		/// <summary>
		/// Character controller
		/// </summary>
		CharacterController m_CharacterController;
		
		/// <summary>
		/// The initial jump velocity
		/// </summary>
		float m_InitialJumpVelocity;

		/// <summary>
		/// The current vertical velocity
		/// </summary>
		/// <returns></returns>
		float m_CurrentVerticalVelocity;
		
		/// <summary>
		/// The current vertical vector
		/// </summary>
		Vector3 m_VerticalVector = Vector3.zero;

		/// <summary>
		/// Stores the grounded-ness of the physics object
		/// </summary>
		bool m_Grounded;
		
		public Action landed { get; set; }
		public Action jumpVelocitySet { get; set; }
		public Action startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }

		/// <inheritdoc />
		public bool isGrounded
		{
			get { return m_Grounded; }
		}
		
		/// <inheritdoc />
		public void Move(Vector3 moveVector3)
		{
			
			m_CharacterController.Move(moveVector3 + m_VerticalVector);
		}

		/// <summary>
		/// Tries to jump
		/// </summary>
		/// <param name="initialVelocity"></param>
		public void SetJumpVelocity(float initialVelocity)
		{
			m_InitialJumpVelocity = initialVelocity;
			if (jumpVelocitySet != null)
			{
				jumpVelocitySet();
			}
		}


		void Awake()
		{
			//Gets the attached character controller
			m_CharacterController = GetComponent<CharacterController>();
			
			//Ensures that the gravity acts downwards
			if (gravity > 0)
			{
				gravity = -gravity;
			}

			if (terminalVelocity > 0)
			{
				terminalVelocity = -terminalVelocity;
			}
		}

		/// <summary>
		/// Handle falling physics
		/// </summary>
		void FixedUpdate()
		{
			AerialMovement();
		}
		
		/// <summary>
		/// Handles Jumping and Falling
		/// </summary>
		void AerialMovement()
		{
			m_Grounded = CheckGrounded();
			
			airTime += Time.fixedDeltaTime;
			m_CurrentVerticalVelocity = Mathf.Clamp(m_InitialJumpVelocity + gravity * airTime, terminalVelocity, Mathf.Infinity);
			float previousFallTime = fallTime;

			if (m_CurrentVerticalVelocity < 0)
			{
				fallTime += Time.fixedDeltaTime;
			}
			
			if (m_CurrentVerticalVelocity < 0f && m_Grounded)
			{
				m_InitialJumpVelocity = 0f;
				m_VerticalVector = Vector3.zero;
				
				//Play the moment that the character lands and only at that moment
				if (Math.Abs(airTime - Time.fixedDeltaTime) > Mathf.Epsilon)
				{
					if (landed != null)
					{
						landed();
					}
				}

				fallTime = 0f;
				airTime = 0f;
				return;
			}
			
			if (previousFallTime < Mathf.Epsilon && fallTime > Mathf.Epsilon)
			{
				if (startedFalling != null)
				{
					startedFalling();
				}
			}
			
			m_VerticalVector = new Vector3(0, m_CurrentVerticalVelocity, 0);
		}
		
		/// <summary>
		/// Checks character controller grounding
		/// </summary>
		bool CheckGrounded()
		{
			Debug.DrawRay(transform.position + m_CharacterController.center, new Vector3(0,-groundCheckDistance * m_CharacterController.height,0), Color.red);
			if (UnityEngine.Physics.Raycast(transform.position + m_CharacterController.center, 
				-transform.up, groundCheckDistance * m_CharacterController.height, groundCheckMask))
			{
				return true;
			}
			return CheckEdgeGrounded();
			
		}

		/// <summary>
		/// Checks character controller edges for ground
		/// </summary>
		bool CheckEdgeGrounded()
		{
			
			Vector3 xRayOffset = new Vector3(m_CharacterController.radius,0f,0f);
			Vector3 zRayOffset = new Vector3(0f,0f,m_CharacterController.radius);		
			
			for (int i = 0; i < 4; i++)
			{
				float sign = 1f;
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
				Debug.DrawRay(transform.position + m_CharacterController.center + sign * rayOffset, 
					new Vector3(0,-groundCheckDistance * m_CharacterController.height,0), Color.blue);

				if (UnityEngine.Physics.Raycast(transform.position + m_CharacterController.center + sign * rayOffset,
					-transform.up,groundCheckDistance * m_CharacterController.height, groundCheckMask))
				{
					return true;
				}
			}
			return false;
		}
	}
	
}
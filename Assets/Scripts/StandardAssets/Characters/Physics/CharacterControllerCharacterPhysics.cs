using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
		/// The distance used to check if grounded
		/// </summary>
		public float groundCheckThreshold = 0.51f;

		/// <summary>
		/// Character controller
		/// </summary>
		CharacterController m_CharacterController;
		
		/// <summary>
		/// The amount of time that the character is in the air for
		/// </summary>
		float m_AirTime;
		
		/// <summary>
		/// The initial jump velocity
		/// </summary>
		float m_InitialJumpVelocity;
		
		/// <summary>
		/// The current vertical vector
		/// </summary>
		Vector3 m_VerticalVector = Vector3.zero;

		/// <summary>
		/// Stores the grounded-ness of the physics object
		/// </summary>
		bool m_Grounded;
		
		public Action landed { get; set; }
		
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
		public void Jump(float initialVelocity)
		{
			m_InitialJumpVelocity = initialVelocity;
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
		}

		/// <summary>
		/// Handle falling physics
		/// </summary>
		void FixedUpdate()
		{
			AerailMovement();
		}
		
		/// <summary>
		/// Handles falling
		/// </summary>
		void AerailMovement()
		{
			m_Grounded = CheckGrounded();
			
			m_AirTime += Time.fixedDeltaTime;

			float currentVerticalVelocity = m_InitialJumpVelocity + gravity * m_AirTime;
			
			
			if (currentVerticalVelocity < 0f && isGrounded)
			{
				m_AirTime = 0f;
				m_InitialJumpVelocity = 0f;
				m_VerticalVector = Vector3.zero;
				if (landed != null)
				{
					landed();
				}
				
				return;
			}
			
			m_VerticalVector = new Vector3(0, currentVerticalVelocity, 0);
		}
		
		/// <summary>
		/// Checks character controller grounding
		/// </summary>
		bool CheckGrounded()
		{
			Debug.DrawRay(transform.position + m_CharacterController.center, new Vector3(0,-groundCheckThreshold * m_CharacterController.height,0), Color.red);
			if (UnityEngine.Physics.Raycast(transform.position + m_CharacterController.center, 
				-transform.up, groundCheckThreshold * m_CharacterController.height))
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
					new Vector3(0,-groundCheckThreshold * m_CharacterController.height,0), Color.blue);

				if (UnityEngine.Physics.Raycast(transform.position + m_CharacterController.center + sign * rayOffset,
					-transform.up,groundCheckThreshold * m_CharacterController.height))
				{
					return true;
				}
			}
			return false;
		}
	}
	
}
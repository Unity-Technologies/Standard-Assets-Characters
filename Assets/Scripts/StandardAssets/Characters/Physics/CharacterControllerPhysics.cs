using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A physic implementation that uses the default Unity character controller
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public class CharacterControllerPhysics : MonoBehaviour, IPhysics
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
		float m_AirTime = 0f;
		
		/// <summary>
		/// The initial jump velocity
		/// </summary>
		float m_InitialJumpVelocity = 0f;
		
		/// <summary>
		/// The current vertical vector
		/// </summary>
		Vector3 m_VerticalVector = Vector3.zero;

		//This is a TEMP solution to the character ungrounding
		private bool m_Grounded;
		
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

		void FixedUpdate()
		{
			Fall();
		}
		
		/// <summary>
		/// Handles falling
		/// </summary>
		void Fall()
		{
			Debug.DrawRay(transform.position + m_CharacterController.center, new Vector3(0,-groundCheckThreshold * m_CharacterController.height,0), Color.red);
			if (UnityEngine.Physics.Raycast(transform.position + m_CharacterController.center, -transform.up, groundCheckThreshold * m_CharacterController.height))
			{
				Debug.Log("Grounded");
				m_Grounded = true;
			}
			else
			{
				m_Grounded = false;
			}
			
			m_AirTime += Time.fixedDeltaTime;

			float currentVerticalVelocity = m_InitialJumpVelocity + gravity * m_AirTime;
			
			
			if (currentVerticalVelocity < 0f && isGrounded)
			{
				m_AirTime = 0f;
				m_InitialJumpVelocity = 0f;
				m_VerticalVector = Vector3.zero;
				return;
			}
			
			m_VerticalVector = new Vector3(0, currentVerticalVelocity, 0);
		}
	}
}
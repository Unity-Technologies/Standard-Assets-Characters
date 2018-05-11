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

		
		
		/// <inheritdoc />
		public void Move(Vector3 moveVector3)
		{
			
			m_CharacterController.Move(moveVector3 + m_VerticalVector);
		}

		/// <inheritdoc />
		public bool isGrounded
		{
			get { return m_CharacterController.isGrounded; }
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
			
			if (isGrounded)
			{
				m_AirTime = 0f;
				m_InitialJumpVelocity = 0f;
				m_VerticalVector = Vector3.zero;
				
				return;
			}
			
			m_AirTime += Time.fixedDeltaTime;

			float currentVerticalVelocity = m_InitialJumpVelocity + gravity * m_AirTime;
			m_VerticalVector = new Vector3(0, currentVerticalVelocity, 0);
			
		}
	}
}
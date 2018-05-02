using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	[RequireComponent(typeof(CharacterController))]
	public class CharacterControllerPhysics : MonoBehaviour, IPhysics
	{
		public float gravity;

		CharacterController m_CharacterController;
		float m_AirTime = 0f;
		float m_InitialJumpVelocity = 0f;
		Vector3 m_VerticalVector = Vector3.zero;
		
		public void Move(Vector3 moveVector3)
		{
			m_CharacterController.Move(moveVector3 + m_VerticalVector);
		}

		public bool canJump
		{
			get { return m_CharacterController.isGrounded; }
		}

		void Awake()
		{
			m_CharacterController = GetComponent<CharacterController>();
			if (gravity > 0)
			{
				gravity = -gravity;
			}
		}

		void FixedUpdate()
		{
			Fall();
		}
		
		void Fall()
		{
			if (canJump)
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
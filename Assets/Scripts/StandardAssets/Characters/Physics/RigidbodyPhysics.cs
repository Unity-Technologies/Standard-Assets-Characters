using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Rigidbody implementation of Physics
	/// Test example of a different Physics implementation
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	public class RigidbodyPhysics : MonoBehaviour, IPhysics
	{
		Rigidbody m_Rigidbody;

		void Awake()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
		}

		public void Move(Vector3 moveVector3)
		{
			m_Rigidbody.MovePosition(transform.position + moveVector3);
		}

		public bool isGrounded
		{
			get { return false; }
		}

		public void Jump(float initialVelocity)
		{
			m_Rigidbody.velocity = m_Rigidbody.velocity + new Vector3(0, initialVelocity, 0);
		}
	}
}
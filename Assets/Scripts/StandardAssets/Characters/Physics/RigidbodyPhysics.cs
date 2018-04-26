using UnityEngine;

namespace StandardAssets.Characters.Physics
{
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

		public bool canJump
		{
			get { return false; }
		}
	}
}
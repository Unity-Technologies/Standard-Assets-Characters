using System;
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
		/// <summary>
		/// The required Rigidbody
		/// </summary>
		Rigidbody m_Rigidbody;

		/// <summary>
		/// Gets Rigidbody on Awake
		/// </summary>
		void Awake()
		{
			isGrounded = true;
			m_Rigidbody = GetComponent<Rigidbody>();
		}

		/// <inheritdoc />
		public void Move(Vector3 moveVector3)
		{
			m_Rigidbody.MovePosition(transform.position + moveVector3);
		}

		public bool isGrounded { get; set; }

		/// <inheritdoc />
		public void Jump(float initialVelocity)
		{
			m_Rigidbody.velocity = m_Rigidbody.velocity + new Vector3(0, initialVelocity, 0);
		}

		/// <inheritdoc />
		public Action lands { get; set; }
	}
}
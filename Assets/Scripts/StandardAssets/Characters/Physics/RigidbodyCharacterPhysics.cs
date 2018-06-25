using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Rigidbody implementation of Physics
	/// Test example of a different Physics implementation
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	public class RigidbodyCharacterPhysics : MonoBehaviour, ICharacterPhysics
	{
		/// <summary>
		/// The required Rigidbody
		/// </summary>
		private Rigidbody rigidBody;
		
		/// <inheritdoc />
		public Action jumpVelocitySet { get; set; }

		public Action<float> startedFalling { get; set; }
		public float airTime { get; private set; }
		public float fallTime { get; private set; }

		/// <inheritdoc />
		public Action landed { get; set; }

		/// <summary>
		/// Gets Rigidbody on Awake
		/// </summary>
		void Awake()
		{
			isGrounded = true;
			rigidBody = GetComponent<Rigidbody>();
		}

		/// <inheritdoc />
		public void Move(Vector3 moveVector)
		{
			rigidBody.MovePosition(transform.position + moveVector);
		}

		public bool isGrounded { get; set; }

		/// <inheritdoc />
		public void SetJumpVelocity(float initialVelocity)
		{
			rigidBody.velocity = rigidBody.velocity + new Vector3(0, initialVelocity, 0);
		}
	}
}
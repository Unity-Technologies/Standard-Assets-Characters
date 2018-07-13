using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Handles the movement of a CharacterCapsule.
	/// </summary>
	public interface ICharacterCapsuleMover
	{
		/// <summary>
		/// Fired when the grounded state changed.
		/// </summary>
		event Action<bool> onGroundedChanged;

		/// <summary>
		/// Fired when the velocity changed.
		/// </summary>
		event Action<Vector3> onVelocityChanged;

		/// <summary>
		/// Fired when the collision flags changed.
		/// </summary>
		event Action<CollisionFlags> onCollisionFlagsChanged;

		/// <summary>
		/// Fired when the stepping over obstacles state changed.
		/// </summary>
		event Action<bool> onStepOverChanged;
		
		/// <summary>
		/// Move the character capsule. This function does not apply any gravity.
		/// </summary>
		/// <param name="moveVector">Move along this vector.</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the Move.</returns>
		CollisionFlags Move(Vector3 moveVector);
	}
}
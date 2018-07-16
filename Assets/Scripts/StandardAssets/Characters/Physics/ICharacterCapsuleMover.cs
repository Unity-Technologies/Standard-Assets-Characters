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
		/// Was touching the ground during the last move, or when the position was set via SetPosition?
		/// </summary>
		bool isGrounded { get; }

		/// <summary>
		/// Collision flags from the last move.
		/// </summary>
		CollisionFlags collisionFlags { get; }

		/// <summary>
		/// Velocity of the last movement. It's the new position minus the old position.
		/// </summary>
		Vector3 velocity { get; }
		
		/// <summary>
		/// Check for small obstacles hitting our side edges when we attempt to step over obstacles? This can be set
		/// to true for the player and false for other characters, because it does additional physics casts.
		/// </summary>
		bool checkSmallObstaclesWhenStepOver { get; set; }
		
		/// <summary>
		/// Move the character capsule. This function does not apply any gravity.
		/// </summary>
		/// <param name="moveVector">Move along this vector.</param>
		/// <returns>CollisionFlags is the summary of collisions that occurred during the Move.</returns>
		CollisionFlags Move(Vector3 moveVector);

		/// <summary>
		/// Move the character. Velocity along the y-axis is ignored. Speed is in units/s. Gravity is automatically applied.
		/// Returns true if the character is grounded. The method will also apply delta time to the speed.
		/// </summary>
		/// <param name="speed">Move along this vector.</param>
		/// <returns>Whether the character is grounded.</returns>
		bool SimpleMove(Vector3 speed);

		/// <summary>
		/// Set the position of the character.
		/// </summary>
		/// <param name="position">Position to set.</param>
		/// <param name="updateGrounded">Update the grounded state? This uses a cast, so only set it to true if you need it.</param>
		void SetPosition(Vector3 position, bool updateGrounded);
	}
}
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
		/// Move the character capsule.
		/// </summary>
		/// <param name="moveVector">Move along this vector.</param>
		void Move(Vector3 moveVector);
	}
}
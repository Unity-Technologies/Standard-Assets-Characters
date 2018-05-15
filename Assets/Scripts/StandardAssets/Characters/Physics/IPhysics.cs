using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A wrapper for the physics controllers so that character controllers are agnostic of the physic implementation
	/// </summary>
	public interface IPhysics
	{
		/// <summary>
		/// Handles movement
		/// </summary>
		/// <param name="moveVector3"></param>
		void Move(Vector3 moveVector3);
		
		/// <summary>
		/// Returns true if the physic objects are grounded
		/// </summary>
		bool isGrounded { get; }

		/// <summary>
		/// Jump with initial velocity
		/// </summary>
		/// <param name="initialVelocity"></param>
		void Jump(float initialVelocity);
	}
}
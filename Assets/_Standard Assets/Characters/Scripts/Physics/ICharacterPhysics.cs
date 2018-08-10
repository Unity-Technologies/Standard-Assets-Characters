using System;
using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// A wrapper for the physics controllers so that character controllers are agnostic of the physic implementation
	/// </summary>
	public interface ICharacterPhysics
	{
		/// <summary>
		/// Returns true if the physic objects are grounded
		/// </summary>
		bool isGrounded { get; }
		
		/// <summary>
		/// Returns true if the character just started sliding down a slope
		/// </summary>
		bool startedSlide { get; }

		/// <summary>
		/// Invoked when the physic object goes from not grounded to grounded
		/// </summary>
		Action landed { get; set; }
		
		/// <summary>
		/// Invoked when the jump velocity is set
		/// </summary>
		Action jumpVelocitySet { get; set; }
		
		/// <summary>
		/// Invoked when the character started falling
		/// </summary>
		Action<float> startedFalling { get; set; }
		
		/// <summary>
		/// The time that the character in is not grounded for
		/// </summary>
		float airTime { get; }
		
		/// <summary>
		/// The time that the character was falling for
		/// </summary>
		float fallTime { get; }

		/// <summary>
		/// The normalized vertical speed
		/// </summary>
		float normalizedVerticalSpeed{ get; }
		
		/// <summary>
		/// Handles movement
		/// </summary>
		/// <param name="moveVector"></param>
		void Move(Vector3 moveVector);
		

		/// <summary>
		/// Jump with initial velocity
		/// </summary>
		/// <param name="initialVelocity"></param>
		void SetJumpVelocity(float initialVelocity);
	}
}
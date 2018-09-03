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
		/// Gets whether the physic objects are grounded
		/// </summary>
		/// <value>Returns true if grounded</value>
		bool isGrounded { get; }
		
		/// <summary>
		/// Gets whether the character just started sliding down a slope.
		/// </summary>
		/// <value>Returns true if the character just started sliding.</value>
		bool startedSlide { get; }

		/// <summary>
		/// Invoked when the physic object goes from not grounded to grounded
		/// </summary>
		event Action landed;

		/// <summary>
		/// Invoked when the jump velocity is set
		/// </summary>
		event Action jumpVelocitySet;

		/// <summary>
		/// Invoked when the character started falling
		/// </summary>
		event Action<float> startedFalling;
		
		/// <summary>
		/// Gets the time that the character in is not grounded for
		/// </summary>
		/// <value>The current aerial time in seconds</value>
		float airTime { get; }
		
		/// <summary>
		/// Gets the time that the character was falling for
		/// </summary>
		/// <value>The current falling aerial time in seconds</value>
		float fallTime { get; }

		/// <summary>
		/// Gets the normalized vertical speed
		/// </summary>
		/// <value>Range = -1 (falling) to 1 (jumping).</value>
		float normalizedVerticalSpeed{ get; }
		
		/// <summary>
		/// Handles movement
		/// </summary>
		/// <param name="moveVector"></param>
		void Move(Vector3 moveVector, float deltaTime);
		

		/// <summary>
		/// Jump with initial velocity
		/// </summary>
		/// <param name="initialVelocity"></param>
		void SetJumpVelocity(float initialVelocity);
	}
}
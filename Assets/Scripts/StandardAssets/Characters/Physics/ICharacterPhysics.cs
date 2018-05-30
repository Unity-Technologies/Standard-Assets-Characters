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
		/// Invoked when the physic object goes from not grounded to grounded
		/// </summary>
		Action landed { get; set; }
		
		/// <summary>
		/// Handles movement
		/// </summary>
		/// <param name="moveVector3"></param>
		void Move(Vector3 moveVector3);
		

		/// <summary>
		/// Jump with initial velocity
		/// </summary>
		/// <param name="initialVelocity"></param>
		void Jump(float initialVelocity);
	}
}
using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Interface for relaying input state to characters (First Person and Third Person)
	/// Consumed by the RootThirdMotionMotor and FirstPersonBrain
	/// Allows developers to change Input from Unity's Input Manager to a different implementation
	/// </summary>
	/// <seealso cref="LegacyCharacterInputBase"/>
	/// <seealso cref="LegacyCharacterInput"/>
	/// <seealso cref="LegacyCrossPlatformCharacterInput"/>
	/// <seealso cref="LegacyOnScreenCharacterInput"/>
	/// <seealso cref="NavMeshAgentInput"/>
	public interface ICharacterInput
	{
		/// <summary>
		/// Used for camera control
		/// </summary>
		/// <remarks>
		/// Range of (-1,-1) to (1,1).
		/// The x value represents horizontal camera movement.
		/// The y value represents vertical camera movement.
		/// </remarks>
		Vector2 lookInput { get; }
		
		/// <summary>
		/// Normalized vector representing how much movement input has been applied.
		/// </summary>
		/// <remarks>
		/// Range of (-1,-1) to (1,1).
		/// The x value represents lateral movement.
		/// The y value represents forward movement (1 = full forward, 0 = being still, -1 = full backwards).
		/// </remarks>
		Vector2 moveInput { get; }
		
		/// <summary>
		/// Helper property for checking that moveInput is non-zero
		/// </summary>
		bool hasMovementInput { get; }
		
		/// <summary>
		///	Helper property for checking if the jump input is held down
		/// </summary>
		bool hasJumpInput { get; }
		
		/// <summary>
		/// Callback raised the moment jump is pressed
		/// </summary>
		Action jumpPressed { get; set; }
	}
}
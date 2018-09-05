using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Interface for relaying input state to characters (First Person and Third Person).
	/// Consumed by the <see cref="StandardAssets.Characters.ThirdPerson.RootMotionThirdPersonMotor"/> and <see cref="StandardAssets.Characters.FirstPerson.FirstPersonBrain"/>.
	/// Allows developers to change Input from Unity's Input Manager to a different implementation.
	/// </summary>
	/// <seealso cref="LegacyCharacterInputBase"/>
	/// <seealso cref="LegacyCharacterInput"/>
	/// <seealso cref="LegacyCrossPlatformCharacterInput"/>
	/// <seealso cref="LegacyOnScreenCharacterInput"/>
	/// <seealso cref="NavMeshAgentInput"/>
	public interface ICharacterInput
	{
		/// <summary>
		/// Gets Normalized vector representing how much camera movement input has been applied.
		/// </summary>
		/// <remarks>
		/// Range of (-1,-1) to (1,1).
		/// </remarks>
		/// <value>Vector2 with range of (-1,-1) to (1,1).</value>
		Vector2 lookInput { get; }
		
		/// <summary>
		/// Gets Normalized vector representing how much movement input has been applied.
		/// </summary>
		/// <remarks>
		/// Range of (-1,-1) to (1,1).
		/// </remarks>
		/// <value>Vector2 with range of (-1,-1) to (1,1).</value>
		Vector2 moveInput { get; }
		
		/// <summary>
		/// Gets bool for if moveInput is non-zero
		/// </summary>
		/// <value>true if movement is applied, false if movement is not applied</value>
		bool hasMovementInput { get; }
		
		/// <summary>
		/// Gets if the jump input is held down
		/// </summary>
		/// <value>true is jump input is held down, false otherwise</value>
		bool hasJumpInput { get; }

		/// <summary>
		/// Callback raised the moment jump is pressed
		/// </summary>
		event Action jumpPressed;
	}
}
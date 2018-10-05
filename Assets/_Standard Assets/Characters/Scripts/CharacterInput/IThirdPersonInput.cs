using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Interface for relaying input state to Third Person Character
	/// </summary>
	public interface IThirdPersonInput
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
	}
}
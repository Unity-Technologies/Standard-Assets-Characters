using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// A mapping between movement values and values used in the animator
	/// </summary>
	public interface IThirdPersonMotor 
	{
		/// <summary>
		/// The turning speed. Range =  -1 (rotate anticlockwise) to 1 (rotate clockwise). 0 is not turning.
		/// </summary>
		float normalizedTurningSpeed { get; }
		
		/// <summary>
		/// The lateral speed. Range = - 1 (strafe left) to 1 (strafe). 0 is no strafing
		/// </summary>
		float normalizedLateralSpeed { get; }
		
		/// <summary>
		/// The forward speed. Range = -1 (run backwards) to 1 (run forwards). 0 is no forward movement 
		/// </summary>
		float normalizedForwardSpeed { get; }
		
		/// <summary>
		/// The time that the character has been falling
		/// </summary>
		float fallTime { get; }
		
		/// <summary>
		/// Fired on jump
		/// </summary>
		Action jumpStarted { get; set; }
		
		/// <summary>
		/// When the character lands
		/// </summary>
		Action landed { get; set; }
		
		/// <summary>
		/// When the starts falling
		/// </summary>
		Action<float> fallStarted { get; set; }
		
		/// <summary>
		/// Fired for a rapid turn
		/// </summary>
		Action<float> rapidlyTurned { get; set; }

		float normalizedVerticalSpeed { get; }

		void Init(ThirdPersonBrain brain);
	}
}

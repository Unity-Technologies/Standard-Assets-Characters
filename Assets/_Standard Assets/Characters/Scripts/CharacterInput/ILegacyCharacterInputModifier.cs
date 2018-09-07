using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Modifies user input received from hardware (e.g. keyboard, mouse, gamepad, touch screen, etc.) It modifies
	/// the input before it is used by the character.
	/// </summary>
	/// <example>
	/// <see cref="StandardAssets.Characters.ThirdPerson.ThirdPersonCharacterInputModifier"/> smooths the input movement 
	/// when rotating in fast circles. This makes the character run in a circle, instead of turning around on the spot.
	/// </example>
	public interface ILegacyCharacterInputModifier
	{
		/// <summary>
		/// This is called after movement input is received from hardware, but before it is used by the character.
		/// The method then modifies the input as needed for the character.
		/// </summary>
		/// <example>
		/// See <see cref="LegacyCharacterInput"/> for a usage example.
		/// </example>
		void ModifyMoveInput(ref Vector2 moveInput);
	}
}
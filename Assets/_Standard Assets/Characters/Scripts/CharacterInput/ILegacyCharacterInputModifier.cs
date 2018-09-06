using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Modifies the input.
	/// </summary>
	public interface ILegacyCharacterInputModifier
	{
		/// <summary>
		/// Modify the the move input.
		/// </summary>
		void ModifyMoveInput(ref Vector2 moveInput);
	}
}
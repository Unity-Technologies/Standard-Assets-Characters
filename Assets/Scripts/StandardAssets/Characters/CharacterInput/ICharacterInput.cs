using System;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Interface for handling character input
	/// </summary>
	public interface ICharacterInput
	{
		Vector2 lookInput { get; }
		Vector2 moveInput { get; }
		bool hasMovementInput { get; }
		Action jumpPressed { get; set; }
	}
}
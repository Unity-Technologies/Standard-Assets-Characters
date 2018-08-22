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
		Vector2 previousNonZeroMoveInput { get; }
		bool hasMovementInput { get; }
		bool isJumping { get; }
		Action jumpPressed { get; set; }
	}
}
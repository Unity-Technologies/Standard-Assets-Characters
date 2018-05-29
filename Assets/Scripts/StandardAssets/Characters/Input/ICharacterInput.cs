using System;
using UnityEngine;

namespace StandardAssets.Characters.Input
{
	/// <summary>
	/// Interface for handling character input
	/// </summary>
	public interface ICharacterInput
	{
		Vector2 moveInput { get; }
		bool isMoveInput { get; }
		Action jump { get; set; }
	}
}
using System;
using UnityEngine;

namespace StandardAssets.Characters.Input
{
	/// <summary>
	/// Interface for handling character input
	/// </summary>
	public interface IInput
	{
		Vector2 moveInput { get; }
		bool isMoveInput { get; }
		Vector2 lookInput { get; }
		Action jump { get; set; }
	}
}
using UnityEngine;

namespace StandardAssets.Characters.Input
{
	public interface IInput
	{
		Vector2 moveInput { get; }
		bool isMoveInput { get; }
	}
}
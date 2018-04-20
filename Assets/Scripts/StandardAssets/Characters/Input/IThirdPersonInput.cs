using UnityEngine;

namespace StandardAssets.Characters.Input
{
	public interface IThirdPersonInput
	{
		Vector2 moveInput { get; }
		bool isMoveInput { get; }
	}
}
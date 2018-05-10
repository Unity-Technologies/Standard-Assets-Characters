using UnityEngine;

namespace StandardAssets.Characters.Input
{
	public interface ILookInput
	{
		Vector2 lookInput { get; }
		bool isLookInput { get; }
	}
}
using UnityEngine;

namespace StandardAssets.Characters.Input
{
	public class DefaultFirstPersonInput : MonoBehaviour, IFirstPersonInput
	{
		public Vector2 moveInput { get; private set; }
		public bool isMoveInput { get; private set; }
	}
}
using StandardAssets.Characters.Effects;
using UnityEngine;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// Abstract bass class for character brains
	/// </summary>
	public abstract class CharacterBrain : MonoBehaviour
	{
		public abstract MovementEventHandler movementEventHandler { get; }
	}
}
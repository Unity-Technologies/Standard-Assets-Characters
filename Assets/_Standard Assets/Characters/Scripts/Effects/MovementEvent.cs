using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Container of data associated with a movement event
	/// </summary>
	public struct MovementEvent
	{
		/// <summary>
		/// Unique identifier for origin of the movement effect. e.g. footstep
		/// </summary>
		public string id;

		/// <summary>
		/// Where the event was fired from
		/// </summary>
		public Transform firedFrom;
	}
}
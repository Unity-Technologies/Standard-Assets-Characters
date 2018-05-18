using System;
using Boo.Lang;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// An entry in the movement event library which maps the MovementEvent unique ID to a set of movement event players
	/// </summary>
	[Serializable]
	public class MovementEventLibraryEntry
	{
		/// <summary>
		/// The Movement unique ID - e.g. footstep
		/// </summary>
		public string id;

		/// <summary>
		/// The list of behaviours for visualize movement events
		/// </summary>
		public MovementEventPlayer[] movementEventPlayers;
	}
}
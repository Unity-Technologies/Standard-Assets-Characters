using System;
using UnityEngine;

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
		[SerializeField, Tooltip("The corresponding Movement Event Id")]
		protected string id;

		/// <summary>
		/// The list of behaviours for visualize movement events
		/// </summary>
		[SerializeField, Tooltip("The effect that occur on a movement event")]
		protected MovementEventPlayer[] movementEventPlayers;

		/// <summary>
		/// Gets the Movement Event ID
		/// </summary>
		public string identifier
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the collection of MovementEventPlayers
		/// </summary>
		public MovementEventPlayer[] movementPlayers
		{
			get { return movementEventPlayers; }
		}
	}
}
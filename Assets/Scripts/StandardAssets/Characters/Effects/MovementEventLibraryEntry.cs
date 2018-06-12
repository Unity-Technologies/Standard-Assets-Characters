using System;
using Boo.Lang;
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
		[SerializeField]
		private string id;

		/// <summary>
		/// The list of behaviours for visualize movement events
		/// </summary>
		[SerializeField]
		private MovementEventPlayer[] movementEventPlayers;

		public string identifier
		{
			get { return id; }
		}

		public MovementEventPlayer[] movementPlayers
		{
			get { return movementEventPlayers; }
		}
	}
}
using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// A library of movement effects.
	/// This is what would be swapped out for different zones
	/// i.e. walking on dirt versus walking on metal
	/// </summary>
	public class MovementEventLibrary : MonoBehaviour
	{
		/// <summary>
		/// The definable list of movement effects
		/// </summary>
		public List<MovementEventLibraryEntry> movementEvents;

		/// <summary>
		/// A dictionary of movement effects for optimized lookup
		/// </summary>
		readonly Dictionary<string, MovementEventLibraryEntry> m_MovementEventsDictionary = new Dictionary<string, MovementEventLibraryEntry>();

		void Awake()
		{
			//Set up the dictionary
			foreach (MovementEventLibraryEntry movementEvent in movementEvents)
			{
				m_MovementEventsDictionary.Add(movementEvent.id, movementEvent);
			}
		}

		/// <summary>
		/// Gets the MovementEventPlayers for a movement event and plays them
		/// </summary>
		/// <param name="movementEvent"></param>
		public void PlayEvent(MovementEvent movementEvent)
		{
			//Play the movement event
			MovementEventLibraryEntry entry = m_MovementEventsDictionary[movementEvent.id];
			foreach (MovementEventPlayer movementEventPlayer in entry.movementEventPlayers)
			{
				movementEventPlayer.Play(movementEvent);
			}
		}
	}
}
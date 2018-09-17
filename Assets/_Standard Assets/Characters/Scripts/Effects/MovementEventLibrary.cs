using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// A library of movement effects.
	/// This is what would be swapped out for different zones.
	/// e.g. walking on dirt versus walking on metal.
	/// </summary>
	public class MovementEventLibrary : MonoBehaviour
	{
		/// <summary>
		/// The definable list of movement effects
		/// </summary>
		[SerializeField, Tooltip("List of Movement Events and their corresponding Movement Event Players")]
		protected List<MovementEventLibraryEntry> movementEvents;

		/// <summary>
		/// A dictionary of movement effects for optimized lookup
		/// </summary>
		private readonly Dictionary<string, MovementEventLibraryEntry> movementEventsDictionary =
			new Dictionary<string, MovementEventLibraryEntry>();

		/// <summary>
		/// Set up dictionary from public list
		/// </summary>
		private void Awake()
		{
			//Set up the dictionary
			foreach (MovementEventLibraryEntry movementEvent in movementEvents)
			{
				movementEventsDictionary.Add(movementEvent.identifier, movementEvent);
			}
		}

		/// <summary>
		/// Gets the MovementEventPlayers for a movement event and plays them
		/// </summary>
		/// <param name="movementEvent"></param>
		public void PlayEvent(MovementEvent movementEvent)
		{
			//Play the movement event
			MovementEventLibraryEntry entry;
			if (movementEventsDictionary.TryGetValue(movementEvent.id, out entry))
			{
				foreach (MovementEventPlayer movementEventPlayer in entry.movementPlayers)
				{
					movementEventPlayer.Play(movementEvent);
				}
			}
		}
	}
}
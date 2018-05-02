using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class MovementEventLibrary : MonoBehaviour
	{
		public List<MovementEventLibraryEntry> movementEvents;

		readonly Dictionary<string, MovementEventLibraryEntry> m_MovementEventsDictionary = new Dictionary<string, MovementEventLibraryEntry>();

		void Awake()
		{
			foreach (MovementEventLibraryEntry movementEvent in movementEvents)
			{
				m_MovementEventsDictionary.Add(movementEvent.id, movementEvent);
			}
		}

		public void PlayEvent(MovementEvent movementEvent)
		{
			MovementEventLibraryEntry entry = m_MovementEventsDictionary[movementEvent.id];
			foreach (MovementEventPlayer movementEventPlayer in entry.movementEventPlayers)
			{
				movementEventPlayer.Play(movementEvent);
			}
		}
	}
}
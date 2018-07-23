using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	[Serializable]
	public abstract class MovementEventHandler
	{
		[SerializeField]
		protected MovementEventLibrary startingMovementEventLibrary;
		
		/// <summary>
		/// The current movement event library
		/// </summary>
		protected MovementEventLibrary movementEventLibrary;

		/// <summary>
		/// Sets the current movement event library
		/// </summary>
		/// <param name="newMovementEventLibrary"></param>
		public void SetCurrentMovementEventLibrary(MovementEventLibrary newMovementEventLibrary)
		{
			movementEventLibrary = newMovementEventLibrary;
		}

		public void Init()
		{
			SetCurrentMovementEventLibrary(startingMovementEventLibrary);
		}
		
		/// <summary>
		/// Plays the movement event
		/// </summary>
		/// <param name="movementEvent"></param>
		protected void OnMoved(MovementEvent movementEvent)
		{
			if (movementEventLibrary == null)
			{
				return;
			}
			
			movementEventLibrary.PlayEvent(movementEvent);
		}
		
		/// <summary>
		/// Helper function for broadcasting events
		/// </summary>
		/// <param name="movementEvent"></param>
		protected virtual void BroadcastMovementEvent(MovementEvent movementEvent)
		{
			OnMoved(movementEvent);
		}

		/// <summary>
		/// Helper function for creating a MovementEvent with a specified id and broadcasting it
		/// </summary>
		/// <param name="id"></param>
		protected virtual void BroadcastMovementEvent(string id)
		{
			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = id;
			BroadcastMovementEvent(movementEvent);
		}
		
		/// <summary>
		/// Helper function for creating a MovementEvent with a specified id and firedFrom transform and broadcasting it
		/// </summary>
		/// <param name="id"></param>
		/// <param name="firedFrom"></param>
		protected virtual void BroadcastMovementEvent(string id, Transform firedFrom)
		{
			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = id;
			movementEvent.firedFrom = firedFrom;
			BroadcastMovementEvent(movementEvent);
		}

	}
}
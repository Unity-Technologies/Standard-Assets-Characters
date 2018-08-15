using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Broadcasts movement events
	/// </summary>
	public abstract class MovementEventBroadcaster : MonoBehaviour
	{
		/// <summary>
		/// Movement event that the MovementEventListener subscribes to
		/// </summary>
		public event Action<MovementEvent> moved;

		/// <summary>
		/// Helper function for broadcasting events
		/// </summary>
		/// <param name="movementEvent"></param>
		protected virtual void BroadcastMovementEvent(MovementEvent movementEvent)
		{
			if (moved != null)
			{
				moved(movementEvent);
			}
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
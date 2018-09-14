using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Abstract class for handling MovementEvents
	/// </summary>
	[Serializable]
	public abstract class MovementEventHandler
	{
		[SerializeField, Tooltip("This is default event library that used")]
		protected MovementEventLibrary startingMovementEventLibrary;
		
		/// <summary>
		/// ID of <see cref="MovementEvent"/> for jump
		/// </summary>
		[SerializeField, Tooltip("The ID of the MovementEventPlayer in the " +
		                         "MovementEventLibrary that will play on this movement event")]
		protected string jumpId = "jump";

		/// <summary>
		/// ID of the <see cref="MovementEvent"/> for landing
		/// </summary>
		[SerializeField, Tooltip("The ID of the MovementEventPlayer in the " +
		                       "MovementEventLibrary that will play on this movement event")]
		protected string landingId = "landing";
		
		/// <summary>
		/// The current Movement event library being used
		/// </summary>
		protected MovementEventLibrary movementEventLibrary;

		/// <summary>
		/// Sets the current <see cref="movementEventLibrary"/>
		/// </summary>
		/// <param name="newMovementEventLibrary">Movement event library data</param>
		public void SetCurrentMovementEventLibrary(MovementEventLibrary newMovementEventLibrary)
		{
			movementEventLibrary = newMovementEventLibrary;
		}
		
		/// <summary>
		/// Sets the movement event library back to starting default
		/// </summary>
		public void SetStartingMovementEventLibrary()
		{
			movementEventLibrary = startingMovementEventLibrary;
		}

		/// <summary>
		/// Sets the current event library to the starting event library
		/// </summary>
		public void Init()
		{
			SetCurrentMovementEventLibrary(startingMovementEventLibrary);
		}
		
		/// <summary>
		/// Helper for playing <see cref="MovementEvent"/> via the current library
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
		
		protected virtual void BroadcastMovementEvent(MovementEvent movementEvent)
		{
			OnMoved(movementEvent);
		}

		/// <summary>
		/// Helper function for creating a MovementEvent with a specified id and broadcasting it
		/// </summary>
		/// <param name="id">The ID of the movement event</param>
		protected virtual void BroadcastMovementEvent(string id)
		{
			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = id;
			BroadcastMovementEvent(movementEvent);
		}
		
		/// <summary>
		/// Helper function for creating a MovementEvent with a specified id and firedFrom transform and broadcasting it
		/// </summary>
		/// <param name="id">The ID of the movement event</param>
		/// <param name="firedFrom">The transform of where the movement event was fire from</param>
		protected virtual void BroadcastMovementEvent(string id, Transform firedFrom)
		{
			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = id;
			movementEvent.firedFrom = firedFrom;
			BroadcastMovementEvent(movementEvent);
		}

		/// <summary>
		/// Helper function for creating a Movement Event with specified id, firedFrom and normalizedSpeed
		/// </summary>
		/// <param name="id">The ID of the movement event</param>
		/// <param name="firedFrom">The transform of where the movement event was fire from</param>
		/// <param name="normalizedSpeed">The normalized speed of the character</param>
		protected virtual void BroadcastMovementEvent(string id, Transform firedFrom, float normalizedSpeed)
		{
			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = id;
			movementEvent.firedFrom = firedFrom;
			movementEvent.normalizedSpeed = normalizedSpeed;
			BroadcastMovementEvent(movementEvent);
		}

	}
}
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
		protected virtual void OnMoved(MovementEvent movementEvent)
		{
			if (moved != null)
			{
				moved(movementEvent);
			}
		}
	}
}
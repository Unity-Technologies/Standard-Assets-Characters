using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public abstract class MovementEventBroadcaster : MonoBehaviour
	{
		public event Action<MovementEvent> moved;

		protected virtual void OnMoved(MovementEvent movementEvent)
		{
			if (moved != null)
			{
				moved(movementEvent);
			}
		}
	}
}
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public abstract class MovementEventPlayer : MonoBehaviour
	{
		public void Play(MovementEvent movementEvent)
		{
			if (movementEvent.firedFrom != null)
			{
				transform.position = movementEvent.firedFrom.position;
				transform.rotation = movementEvent.firedFrom.rotation;
			}
			
			PlayMovementEvent(movementEvent);
		}

		protected abstract void PlayMovementEvent(MovementEvent movementEvent);
	}
}
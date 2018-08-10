using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Abstract base class for visualizing a movement event
	/// </summary>
	public abstract class MovementEventPlayer : MonoBehaviour
	{
		/// <summary>
		/// Plays the movement event at a set location
		/// </summary>
		/// <param name="movementEvent"></param>
		public void Play(MovementEvent movementEvent)
		{
			if (movementEvent.firedFrom != null)
			{
				transform.position = movementEvent.firedFrom.position;
				transform.rotation = movementEvent.firedFrom.rotation;
			}
			
			PlayMovementEvent(movementEvent);
			
		}

		/// <summary>
		/// Does the actual playing of the event
		/// </summary>
		/// <param name="movementEvent"></param>
		protected abstract void PlayMovementEvent(MovementEvent movementEvent);
	}
}
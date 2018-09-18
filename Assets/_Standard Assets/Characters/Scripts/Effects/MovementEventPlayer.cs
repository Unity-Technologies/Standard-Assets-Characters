using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Abstract base class for visualizing a movement event
	/// </summary>
	public abstract class MovementEventPlayer : MonoBehaviour
	{
		[SerializeField, Tooltip("Should rotation be set on play")]
		protected bool setRotation;
		
		/// <summary>
		/// Plays the movement event at a set location
		/// </summary>
		/// <param name="movementEventData"></param>
		public void Play(MovementEventData movementEventData)
		{
			if (movementEventData.firedFrom != null)
			{
				transform.position = movementEventData.firedFrom.position;
				if (setRotation)
				{
					transform.rotation = movementEventData.firedFrom.rotation;
				}
			}

			PlayMovementEvent(movementEventData);
		}

		/// <summary>
		/// Does the actual playing of the event
		/// </summary>
		/// <param name="movementEventData"></param>
		protected abstract void PlayMovementEvent(MovementEventData movementEventData);
	}
}
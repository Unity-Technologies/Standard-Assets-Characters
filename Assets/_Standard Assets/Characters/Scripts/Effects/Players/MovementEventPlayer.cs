using UnityEngine;

namespace StandardAssets.Characters.Effects.Players
{
	/// <summary>
	/// Abstract base class for visualizing a movement event
	/// </summary>
	public abstract class MovementEventPlayer : MonoBehaviour
	{
		[SerializeField, Tooltip("Should rotation be set on play?")]
		bool m_SetRotation;
		

		/// <summary>
		/// Plays the movement event at a set location
		/// </summary>
		/// <param name="movementEventData"></param>
		public void Play(MovementEventData movementEventData)
		{
			if (movementEventData.firedFrom != null)
			{
				transform.position = movementEventData.firedFrom.position;
				if (m_SetRotation)
				{
					transform.rotation = movementEventData.firedFrom.rotation;
				}
			}

			PlayMovementEvent(movementEventData);
		}
		
		/// <summary>
		/// Intercepts the movement event call, calculates and appends the effect magnitude
		/// </summary>
		/// <param name="movementEventData">The <see cref="MovementEventData"/> that is intercepted</param>
		protected void PlayMovementEvent(MovementEventData movementEventData)
		{
			var effectMagnitude = Evaluate(movementEventData.normalizedSpeed);
			
			PlayMovementEvent(movementEventData, effectMagnitude);
		}

		/// <summary>
		/// Plays the event using the effectMagnitude
		/// </summary>
		/// <param name="movementEventData">The current <see cref="MovementEventData"/> to be played</param>
		/// <param name="effectMagnitude">The magnitude of the effect - this is the actual value and not a normalized value</param>
		protected abstract void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude);

		/// <summary>
		/// Abstract method for calculating the effect magnitude based on the normalizedSpeed
		/// </summary>
		protected abstract float Evaluate(float normalizedSpeed);
	}
}
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Base for movement players that consume the normalized speed of the character
	/// </summary>
	public abstract class NormalizedSpeedMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField, Tooltip("How the effect is scaled based on normalizedSpeed")]
		protected AnimationCurve normalizedEffectMagnitudeBasedOnNormalizedSpeed = AnimationCurve.Linear(0f,0f,1f,1f);

		/// <summary>
		/// Gets the minimum value of the effect. i.e. where <see cref="normalizedEffectMagnitudeBasedOnNormalizedSpeed"/> is zero
		/// </summary>
		protected abstract float minValue { get; }
		
		/// <summary>
		/// Gets the maximum value of the effect. i.e. where where <see cref="normalizedEffectMagnitudeBasedOnNormalizedSpeed"/> is 1
		/// </summary>
		protected abstract float maxValue { get; }
		
		/// <summary>
		/// Intercepts the movement event call, calculates and appends the effect magnitude
		/// </summary>
		/// <param name="movementEventData">The <see cref="MovementEventData"/> that is intercepted</param>
		protected override void PlayMovementEvent(MovementEventData movementEventData)
		{
			float effectMagnitude =
				normalizedEffectMagnitudeBasedOnNormalizedSpeed.Evaluate(movementEventData.normalizedSpeed) * (maxValue - minValue) + minValue;
			
			PlayMovementEvent(movementEventData, effectMagnitude);
		}

		/// <summary>
		/// Plays the event using the effectMagnitude
		/// </summary>
		/// <param name="movementEventData">The current <see cref="MovementEventData"/> to be played</param>
		/// <param name="effectMagnitude">The magnitude of the effect - this is the actual value and not a normalized value</param>
		protected abstract void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude);
	}
}
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Base for movement players that consume the normalized speed of the character
	/// </summary>
	public abstract class NormalizedSpeedMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField, Tooltip("How the effect is scaled based on normalizedSpeed")]
		protected AnimationCurve normalizedEffectMagnitudeBasedOnNormalizedSpeed = AnimationCurve.Linear(0,0,1,1);

		/// <summary>
		/// Gets the minimum value of the effect. i.e. where normalizedEffectMagnitudeBasedOnNormalizedSpeed is zero
		/// </summary>
		protected abstract float minValue { get; }
		
		/// <summary>
		/// Gets the maximum value of the effect. i.e. where normalizedEffectMagnitudeBasedOnNormalizedSpeed is 1
		/// </summary>
		protected abstract float maxValue { get; }
		
		/// <summary>
		/// Intercepts the movement event call, calculates and appends the effect magnitude
		/// </summary>
		/// <param name="movementEvent">The <see cref="MovementEvent"/> that is intercepted</param>
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			float effectMagnitude =
				normalizedEffectMagnitudeBasedOnNormalizedSpeed.Evaluate(movementEvent.normalizedSpeed) * (maxValue - minValue) + minValue;
			
			PlayMovementEvent(movementEvent, effectMagnitude);
		}

		/// <summary>
		/// Plays the event using the effectMagnitude
		/// </summary>
		/// <param name="movementEvent">The current <see cref="MovementEvent"/> to be played</param>
		/// <param name="effectMagnitude">The magnitude of the effect - this is the actual value and not a normalized value</param>
		protected abstract void PlayMovementEvent(MovementEvent movementEvent, float effectMagnitude);
	}
}
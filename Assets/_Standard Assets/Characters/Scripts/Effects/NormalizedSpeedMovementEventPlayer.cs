using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public abstract class NormalizedSpeedMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField]
		protected AnimationCurve normalizedEffectMagnitudeBasedOnNormalizedSpeed = AnimationCurve.Linear(0,0,1,1);

		protected abstract float minValue { get; }
		
		protected abstract float maxValue { get; }
		
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			float effectMagnitude =
				normalizedEffectMagnitudeBasedOnNormalizedSpeed.Evaluate(movementEvent.normalizedSpeed) * (maxValue - minValue) + minValue;
			
			PlayMovementEvent(movementEvent, effectMagnitude);
		}

		protected abstract void PlayMovementEvent(MovementEvent movementEvent, float effectMagnitude);
	}
}
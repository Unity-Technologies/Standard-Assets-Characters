using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Calls play on the ParticleSystem and scales the transform based on the effectMagnitude
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField, Tooltip("How the particle system is scaled based on normalizedSpeed")]
		protected AnimationCurve particleScaleFromNormalizedSpeed = AnimationCurve.Linear(0f,0f,1f,1f);
		
		private ParticleSystem[] particleSources;

		private void Awake()
		{
			particleSources = GetComponentsInChildren<ParticleSystem>();
		}

		/// <summary>
		/// Plays the particles and scales them based on magnitude
		/// </summary>
		/// <param name="movementEventData">The data driving the movement event</param>
		/// <param name="effectMagnitude">The magnitude of the effect used for scaling the particle system size</param>
		protected override void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude)
		{
			Vector3 scale = Vector3.one * effectMagnitude;
			foreach (ParticleSystem particleSource in particleSources)
			{
				particleSource.transform.localScale = scale;
				particleSource.Play();
			}
		}
		
		protected override float Evaluate(float normalizedSpeed)
		{
			return particleScaleFromNormalizedSpeed.Evaluate(normalizedSpeed);
		}
	}
}

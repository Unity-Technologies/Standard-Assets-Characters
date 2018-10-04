using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Helpers;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Calls play on the ParticleSystem and scales the transform based on the effectMagnitude
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMovementEventPlayer : NormalizedSpeedMovementEventPlayer
	{
		[SerializeField, FloatRangeSetup(0f, 5f), Tooltip("The local scale range of the particle systems")]
		protected FloatRange scale;
		
		private ParticleSystem[] particleSources;

		private void Awake()
		{
			particleSources = GetComponentsInChildren<ParticleSystem>();
		}

		protected override float minValue
		{
			get { return scale.minValue; }
		}

		protected override float maxValue
		{
			get { return scale.maxValue; }
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
	}
}

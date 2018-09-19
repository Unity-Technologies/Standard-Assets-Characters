using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Calls play on the ParticleSystem and scales the transform based on the effectMagnitude
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMovementEventPlayer : NormalizedSpeedMovementEventPlayer
	{
		[SerializeField, Tooltip("The maximum local scale of the particle player object")]
		protected float maximumLocalScale = 1f;

		[SerializeField, Tooltip("The minimum local scale of the particle player object")]
		protected float minimumLocalScale;
		
		private ParticleSystem[] particleSources;

		private void Awake()
		{
			particleSources = GetComponentsInChildren<ParticleSystem>();
		}

		protected override float minValue
		{
			get { return minimumLocalScale; }
		}

		protected override float maxValue
		{
			get { return maximumLocalScale; }
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

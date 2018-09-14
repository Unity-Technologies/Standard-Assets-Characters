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

		/// <summary>
		/// Gets the minimumLocalScale as the minValue which is used to calculate the effectMagnitude in <see cref="NormalizedSpeedMovementEventPlayer"/>
		/// </summary>
		protected override float minValue
		{
			get { return minimumLocalScale; }
		}

		/// <summary>
		/// Gets the maximumLocalScale as the maxValue which is used to calculate the effectMagnitude in <see cref="NormalizedSpeedMovementEventPlayer"/>
		/// </summary>
		protected override float maxValue
		{
			get { return maximumLocalScale; }
		}

		protected override void PlayMovementEvent(MovementEvent movementEvent, float effectMagnitude)
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

using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMovementEventPlayer : NormalizedSpeedMovementEventPlayer
	{
		[SerializeField, Tooltip("The maximum local scale of the particle player object")]
		protected float maximumLocalScale = 1f;

		[SerializeField, Tooltip("The minimum local scale of the particle player object")]
		protected float minimumLocalScale;
		
		private ParticleSystem particleSource;
		private float minValue1;
		private float maxValue1;

		private void Awake()
		{
			particleSource = GetComponent<ParticleSystem>();
		}

		protected override float minValue
		{
			get { return maximumLocalScale; }
		}

		protected override float maxValue
		{
			get { return minimumLocalScale; }
		}

		protected override void PlayMovementEvent(MovementEvent movementEvent, float effectMagnitude)
		{
			transform.localScale = Vector3.one * effectMagnitude;
			particleSource.Play();
		}
	}
}

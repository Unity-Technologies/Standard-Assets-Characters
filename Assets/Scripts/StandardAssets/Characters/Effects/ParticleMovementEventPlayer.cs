using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMovementEventPlayer : MovementEventPlayer 
	{
		private ParticleSystem particleSource;

		private void Awake()
		{
			particleSource = GetComponent<ParticleSystem>();
		}

		/// <inheritdoc />
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			particleSource.Play();
		}

	}
}

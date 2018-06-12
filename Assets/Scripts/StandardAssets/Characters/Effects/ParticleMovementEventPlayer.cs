using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	public class ParticleMovementEventPlayer : MovementEventPlayer 
	{
		/// <summary>
		/// Particles to be emitted
		/// </summary>
		[SerializeField]
		private ParticleSystem particleSource;
		
		/// <inheritdoc />
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			particleSource.Play();
		}

	}
}

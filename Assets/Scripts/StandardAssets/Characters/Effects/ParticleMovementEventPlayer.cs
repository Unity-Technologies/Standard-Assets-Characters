using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class ParticleMovementEventPlayer : MovementEventPlayer 
	{
		/// <summary>
		/// Particles to be emitted
		/// </summary>
		public ParticleSystem partSource;
		
		/// <inheritdoc />
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			partSource.Play();
		}

	}
}

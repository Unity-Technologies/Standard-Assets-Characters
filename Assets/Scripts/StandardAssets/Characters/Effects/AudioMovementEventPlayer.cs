using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class AudioMovementEventPlayer : MovementEventPlayer
	{
		public AudioSource source;
		
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			source.Play();
		}
	}
}
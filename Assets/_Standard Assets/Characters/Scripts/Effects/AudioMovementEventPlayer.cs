using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	public class AudioMovementEventPlayer : MovementEventPlayer
	{
		/// <summary>
		/// The audio source to be played
		/// </summary>
		[SerializeField, Tooltip("When using a single audio source")]
		protected AudioSource source;

		/// <summary>
		/// Collection of audio clips that could be randomly selected by this <see cref="AudioMovementEventPlayer"/>
		/// </summary>
		[SerializeField, Tooltip("For using multiple audio sources, i.e footstep sounds")]
		protected AudioClip[] clips;

		private int currentSoundIndex;

		private void Awake()
		{
			currentSoundIndex = 0;
		}

		/// <summary>
		/// Play the audio source associated with the movement event or cycle
		/// through multiple clips if required. e.g. for alternating footstep sounds. 
		/// </summary>
		/// <param name="movementEvent">Movement event data</param>
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			if (source == null)
			{
				return;
			}

			if (clips.Length != 0)
			{
				if (currentSoundIndex >= clips.Length)
				{
					currentSoundIndex = 0;
				}
				source.clip = clips[currentSoundIndex++];
			}
			
			source.Play();
		}
	}
}
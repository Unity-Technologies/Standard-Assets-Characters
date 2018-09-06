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
		/// Audio clips to use for cycling through clips, i.e as foot steps
		/// </summary>
		[SerializeField, Tooltip("For using multiple audio sources, i.e footstep sounds")]
		protected AudioClip[] clips;

		private int currentSoundIndex;

		private void Awake()
		{
			currentSoundIndex = 0;
		}

		/// <summary>
		/// Play movement events
		/// </summary>
		/// <param name="movementEvent">Movement event data</param>
		protected override void PlayMovementEvent(MovementEvent movementEvent)
		{
			source.clip = clips[currentSoundIndex];

			currentSoundIndex++;
			if (currentSoundIndex >= clips.Length)
			{
				currentSoundIndex = 0;
			}

			if (source != null)
			{
				source.Play();
			}
		}
	}
}
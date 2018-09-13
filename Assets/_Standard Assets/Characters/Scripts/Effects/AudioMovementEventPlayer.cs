using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	public class AudioMovementEventPlayer : NormalizedSpeedMovementEventPlayer
	{
		/// <summary>
		/// The audio source to be played
		/// </summary>
		[SerializeField, Tooltip("The AudioSource used to play the selected clip")]
		protected AudioSource source;

		/// <summary>
		/// Collection of audio clips that could be randomly selected by this <see cref="AudioMovementEventPlayer"/>
		/// </summary>
		[SerializeField, Tooltip("For using multiple audio sources, i.e footstep sounds")]
		protected AudioClip[] clips;

		[SerializeField, Tooltip("The maximum volume that the clip is played at"), Range(0f, 1f)]
		protected float maximumVolume = 1f;
		
		[SerializeField, Tooltip("The minimum volume that the clip is played at"), Range(0f, 1f)]
		protected float minimumVolume;
		

		private int currentSoundIndex;

		private void Awake()
		{
			currentSoundIndex = 0;
		}

		protected override float minValue
		{
			get { return minimumVolume; }
		}

		protected override float maxValue
		{
			get { return maximumVolume; }
		}

		protected override void PlayMovementEvent(MovementEvent movementEvent, float effectMagnitude)
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
				source.PlayOneShot(clips[currentSoundIndex++], effectMagnitude);
				return;
			}

			source.volume = effectMagnitude;
			source.Play();

		}
	}
}
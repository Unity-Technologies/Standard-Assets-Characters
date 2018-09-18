using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Selects an audio clip to play and scales the volume based on character speed
	/// </summary>
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

		/// <summary>
		/// The maximum volume that the clip is played at
		/// </summary>
		[SerializeField, Tooltip("The maximum volume that the clip is played at"), Range(0f, 1f)]
		protected float maximumVolume = 1f;
		
		/// <summary>
		/// The minimum volume that the clip is played at
		/// </summary>
		[SerializeField, Tooltip("The minimum volume that the clip is played at"), Range(0f, 1f)]
		protected float minimumVolume;

		private int currentSoundIndex;

		protected override float minValue
		{
			get { return minimumVolume; }
		}

		protected override float maxValue
		{
			get { return maximumVolume; }
		}
		
		private void Awake()
		{
			currentSoundIndex = 0;
		}

		/// <summary>
		/// Selects an audio clip (by cycling through them) and changes the volume based on <paramref name="effectMagnitude"/>
		/// </summary>
		/// <param name="movementEventData">The <see cref="MovementEventData"/> data</param>
		/// <param name="effectMagnitude">The magnitude of the effect</param>
		protected override void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude)
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
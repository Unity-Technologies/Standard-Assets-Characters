using UnityEngine;

namespace StandardAssets.Characters.Effects.Players
{
	/// <summary>
	/// Selects an audio clip to play and scales the volume based on character speed
	/// </summary>
	public class AudioMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField, Tooltip("How the volume is scaled based on normalizedSpeed")]
		protected AnimationCurve volumeFromNormalizedSpeed = AnimationCurve.Linear(0f,0f,1f,1f);
		
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

		private int currentSoundIndex;
		
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

		protected override float Evaluate(float normalizedSpeed)
		{
			return volumeFromNormalizedSpeed.Evaluate(normalizedSpeed);
		}
	}
}
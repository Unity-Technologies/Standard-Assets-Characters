using UnityEngine;

namespace StandardAssets.Characters.Effects.Players
{
	/// <summary>
	/// Selects an audio clip to play and scales the volume based on character speed
	/// </summary>
	public class AudioMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField, Tooltip("Curve for scaling volume based on normalizedSpeed")]
		AnimationCurve m_VolumeCurve = AnimationCurve.Linear(0f,0f,1f,1f);
		
		[SerializeField, Tooltip("AudioSource used to play the selected clip")]
		AudioSource m_Source;

		[SerializeField, Tooltip("For using multiple audio sources, i.e footstep sounds")]
		AudioClip[] m_Clips;

		// Index of the current audio clip
		int m_CurrentSoundIndex;


		// Set the index to 0
		void Awake()
		{
			m_CurrentSoundIndex = 0;
		}

		/// <summary>
		/// Selects an audio clip (by cycling through them) and changes the volume based on <paramref name="effectMagnitude"/>
		/// </summary>
		/// <param name="movementEventData">The <see cref="MovementEventData"/> data</param>
		/// <param name="effectMagnitude">The magnitude of the effect</param>
		protected override void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude)
		{
			if (m_Source == null)
			{
				return;
			}

			if (m_Clips.Length != 0)
			{
				if (m_CurrentSoundIndex >= m_Clips.Length)
				{
					m_CurrentSoundIndex = 0;
				}
				m_Source.PlayOneShot(m_Clips[m_CurrentSoundIndex++], effectMagnitude);
				return;
			}

			m_Source.volume = effectMagnitude;
			m_Source.Play();
		}

		/// <summary>
		/// Returns the volume based on the normalizedSpeed
		/// </summary>
		protected override float Evaluate(float normalizedSpeed)
		{
			return m_VolumeCurve.Evaluate(normalizedSpeed);
		}
	}
}
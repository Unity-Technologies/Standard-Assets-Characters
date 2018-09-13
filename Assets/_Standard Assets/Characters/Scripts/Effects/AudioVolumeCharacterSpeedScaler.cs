using StandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Scales the volume of an audio source based off the <see cref="thirdPersonBrain"/> normalized speed
	/// </summary>
	public class AudioVolumeCharacterSpeedScaler : CharacterSpeedEffectScaler
	{
		/// <summary>
		/// The audio source to be volume scaled
		/// </summary>
		[SerializeField]
		protected AudioSource source;

		[SerializeField]
		protected float minimumVolume = 0.1f;
		
		private float scaledVolume;

		private float defaultVolume;
		
		protected override void Awake()
		{
			base.Awake();
			
			if (source != null)
			{
				defaultVolume = source.volume;
			}
		}

		protected override void ApplyNormalizedSpeedToEffect(float normalizedSpeedToApply)
		{
			scaledVolume = defaultVolume * normalizedSpeedToApply;
			
			if (scaledVolume < minimumVolume)
			{
				scaledVolume = minimumVolume;
			}
			
			source.volume = scaledVolume;
		}
	}
}

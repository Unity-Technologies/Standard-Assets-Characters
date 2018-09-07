using StandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Scales the volume of an audio source based off the <see cref="thirdPersonBrain"/> normalized speed
	/// </summary>
	public class AudioVolumeCharacterSpeedScaler : MonoBehaviour
	{
		/// <summary>
		/// The audio source to be volume scaled
		/// </summary>
		[SerializeField]
		protected AudioSource source;

		[SerializeField]
		protected ThirdPersonBrain thirdPersonBrain;

		private float scaledVolume;
		
		private float defaultVolume;
		
		private void Awake()
		{
			if (source != null)
			{
				defaultVolume = source.volume;
			}
		}
		
		void Update()
		{
			if (thirdPersonBrain != null && thirdPersonBrain.isActiveAndEnabled)
			{
				ScaleVolumeToSpeed();
			}
		}

		private void ScaleVolumeToSpeed()
		{
			if (source == null)
			{
				return;
			}
			
			if (thirdPersonBrain.currentMotor.normalizedForwardSpeed == 0)
			{
				scaledVolume = defaultVolume * thirdPersonBrain.currentMotor.normalizedLateralSpeed;
			}
			else
			{
				scaledVolume = defaultVolume * thirdPersonBrain.normalizedForwardSpeed;
			}
		
			if (scaledVolume < 0)
			{
				scaledVolume *= -1;
			}
			
			source.volume = scaledVolume;
		}
	}
}

using StandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class AudioVolumeCharacterSpeedScaler : MonoBehaviour
	{
		/// <summary>
		/// The audio source to be played
		/// </summary>
		[SerializeField]
		protected AudioSource source;

		[SerializeField]
		protected ThirdPersonBrain thirdPersonBrain;
		
		private float volume;
		
		private void Awake()
		{
			volume = source.volume;
		}
		
		void Update()
		{
			
			if (thirdPersonBrain != null)
			{
				ScaleVolumeToSpeed();
			}
		}

		private void ScaleVolumeToSpeed()
		{
			float scaledVolume = 0;
			
			if (thirdPersonBrain.CurrentMotor.normalizedLateralSpeed == 0)
			{
				scaledVolume = volume * thirdPersonBrain.CurrentMotor.normalizedForwardSpeed;
			}
			else
			{
				scaledVolume = volume * thirdPersonBrain.CurrentMotor.normalizedLateralSpeed;
			}
		
			if (scaledVolume < 0)
			{
				scaledVolume *= -1;
			}
			
			source.volume = scaledVolume;
		}
	}
}
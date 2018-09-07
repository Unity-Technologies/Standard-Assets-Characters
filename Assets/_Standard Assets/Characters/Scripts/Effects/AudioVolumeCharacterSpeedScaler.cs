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

		private float scaledVolume;
		
		private float defaultVolume;
		
		private void Awake()
		{
			defaultVolume = source.volume;
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
			if (thirdPersonBrain.CurrentMotor.normalizedLateralSpeed == 0)
			{
				scaledVolume = defaultVolume * thirdPersonBrain.CurrentMotor.normalizedForwardSpeed;
			}
			else
			{
				scaledVolume = defaultVolume * thirdPersonBrain.CurrentMotor.normalizedLateralSpeed;
			}
		
			if (scaledVolume < 0)
			{
				scaledVolume *= -1;
			}
			
			source.volume = scaledVolume;
		}
	}
}

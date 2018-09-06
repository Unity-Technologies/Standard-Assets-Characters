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

		public float updatedVolume
		{
			get
			{
				return scaledVolume;
			}
		}
		
		private float volume;
		
		private void Awake()
		{
			volume = source.volume;
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
			if (thirdPersonBrain.currentMotor.normalizedLateralSpeed == 0)
			{
				scaledVolume = volume * thirdPersonBrain.currentMotor.normalizedForwardSpeed;
			}
			else
			{
				scaledVolume = volume * thirdPersonBrain.currentMotor.normalizedLateralSpeed;
			}
		
			if (scaledVolume < 0)
			{
				scaledVolume *= -1;
			}
			
			//source.volume = scaledVolume;
		}
	}
}
using StandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Scales the particle start size based off the <see cref="thirdPersonBrain"/> character speed
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleSizeCharacterSpeedScaler: MonoBehaviour
	{
		[SerializeField, Tooltip("Third person brain for getting character speed")]
		protected ThirdPersonBrain thirdPersonBrain;

		private ParticleSystem particleSystem;

		private float maxParticleStartSize;

		private float scaledParticleStartSize;

		private void Awake()
		{
			particleSystem = GetComponent<ParticleSystem>();
			maxParticleStartSize = particleSystem.main.startSize.constant;
		}

		private void Update()
		{
			ScaleParticleSizeToSpeed();
		}

		private void ScaleParticleSizeToSpeed()
		{
			if (thirdPersonBrain == null || maxParticleStartSize == null)
			{
				return;
			}
			if (thirdPersonBrain.currentMotor.normalizedForwardSpeed == 0)
			{
				scaledParticleStartSize = maxParticleStartSize * thirdPersonBrain.currentMotor.normalizedLateralSpeed;
			}
			else
			{
				scaledParticleStartSize = maxParticleStartSize * thirdPersonBrain.normalizedForwardSpeed;
			}
			
			scaledParticleStartSize = Mathf.Abs(scaledParticleStartSize);

			if (scaledParticleStartSize < 1)
			{
				scaledParticleStartSize = 1;
			}
			ParticleSystem.MainModule particleSystemMain = particleSystem.main;
			particleSystemMain.startSize =  scaledParticleStartSize;
		}
	}
}
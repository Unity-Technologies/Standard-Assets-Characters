using UnityEngine;
using Util;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Scales the particle start size based off the <see cref="thirdPersonBrain"/> character speed
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleSizeCharacterSpeedScaler : CharacterSpeedEffectScaler
	{
		[SerializeField]
		protected AnimationCurve particleScaleFromNormalizedSpeed = AnimationCurve.Linear(0,0,1,1);

		[SerializeField]
		protected float minScale = 1f;

		[SerializeField]
		protected float minSpeedForParticleEmission = 0.5f;

		private float maxScale;
		
		private ParticleSystem cachedParticleSystem;

		private float scaledParticleStartSize;

		protected override void Awake()
		{
			base.Awake();
			cachedParticleSystem = GetComponent<ParticleSystem>();
			maxScale = cachedParticleSystem.main.startSize.constant;
		}

		protected override void ApplyNormalizedSpeedToEffect(float normalizedSpeedToApply)
		{
			ParticleSystem.MainModule particleSystemMain = cachedParticleSystem.main;
			if (normalizedSpeedToApply < minSpeedForParticleEmission)
			{
				particleSystemMain.startSize = 0f;
				return;
			}
			scaledParticleStartSize = particleScaleFromNormalizedSpeed.Evaluate(normalizedSpeedToApply) * (maxScale - minScale) + minScale;
			particleSystemMain.startSize = scaledParticleStartSize;
		}
	}
}
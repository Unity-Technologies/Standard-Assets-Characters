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

		private float maxScale;
		
		private ParticleSystem particleSystem;

		private float scaledParticleStartSize;

		protected override void Awake()
		{
			base.Awake();
			particleSystem = GetComponent<ParticleSystem>();
			maxScale = particleSystem.main.startSize.constant;
		}

		protected override void ApplyNormalizedSpeedToEffect(float normalizedSpeed)
		{
			ParticleSystem.MainModule particleSystemMain = particleSystem.main;
			scaledParticleStartSize = particleScaleFromNormalizedSpeed.Evaluate(normalizedSpeed) * (maxScale - minScale) + minScale;
			particleSystemMain.startSize = scaledParticleStartSize;
		}
	}
}
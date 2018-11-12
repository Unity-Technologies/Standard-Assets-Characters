using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.Effects.Players
{
	/// <summary>
	/// Calls play on the ParticleSystem and scales the transform based on the effectMagnitude
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMovementEventPlayer : MovementEventPlayer
	{
		[FormerlySerializedAs("particleScaleFromNormalizedSpeed")]
		[SerializeField, Tooltip("Curve used to scale the particle system based on normalizedSpeed")]
		AnimationCurve m_ParticleScaleFromNormalizedSpeed = AnimationCurve.Linear(0f,0f,1f,1f);

		ParticleSystem[] m_ParticleSystems;

		void Awake()
		{
			m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
		}

		/// <summary>
		/// Plays the particles and scales them based on magnitude
		/// </summary>
		/// <param name="movementEventData">The data driving the movement event</param>
		/// <param name="effectMagnitude">The magnitude of the effect used for scaling the particle system size</param>
		protected override void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude)
		{
			var scale = Vector3.one * effectMagnitude;
			foreach (var particleSystem in m_ParticleSystems)
			{
				particleSystem.transform.localScale = scale;
				particleSystem.Play();
			}
		}
		
		protected override float Evaluate(float normalizedSpeed)
		{
			return m_ParticleScaleFromNormalizedSpeed.Evaluate(normalizedSpeed);
		}
	}
}

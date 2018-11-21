using UnityEngine;

namespace StandardAssets.Characters.Effects.Players
{
	/// <summary>
	/// Calls play on the ParticleSystem and scales the transform based on the effectMagnitude
	/// </summary>
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField, Tooltip("Curve used to scale the particle system based on normalizedSpeed")]
		AnimationCurve m_ScaleCurve = AnimationCurve.Linear(0f,0f,1f,1f);

		// Particle Systems used in effect
		ParticleSystem[] m_ParticleSystems;


		// Finds Particle Systems for use in effect
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
		
		/// <summary>
		/// Calculates the size of the particle systems 
		/// </summary>
		protected override float Evaluate(float normalizedSpeed)
		{
			return m_ScaleCurve.Evaluate(normalizedSpeed);
		}
	}
}

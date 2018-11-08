using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.Effects.Players
{
	/// <summary>
	/// Plays multiple <see cref="MovementEventPlayer"/> simultaneously
	/// </summary>
	public class CompoundMovementEventPlayer : MovementEventPlayer
	{
		[FormerlySerializedAs("playerPrefabs")]
		[SerializeField, Tooltip("The prefabs of the movement event players - these are spawned once and played")]
		MovementEventPlayer[] m_PlayerPrefabs;

		/// <summary>
		/// A cache of the spawned multiple <see cref="MovementEventPlayer"/> instances 
		/// </summary>
		MovementEventPlayer[] m_PlayerInstances;

		/// <summary>
		/// Initializes, caches and plays the multiple <see cref="MovementEventPlayer"/>
		/// </summary>
		/// <param name="movementEventData">the movement event data model</param>
		/// <param name="effectMagnitude">the magnitude of the effect</param>
		protected override void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude)
		{
			//If the cache does exist or is empty then setup the cache and play the sounds
			if (m_PlayerInstances == null || m_PlayerInstances.Length == 0)
			{
				m_PlayerInstances = new MovementEventPlayer[m_PlayerPrefabs.Length];
				var i = -1;
				foreach (var movementEventPlayer in m_PlayerPrefabs)
				{
					i++;
					var instance = Instantiate(movementEventPlayer, transform);
					m_PlayerInstances[i] = instance;
					instance.Play(movementEventData);
				}
			}
			//otherwise just play the cached effects
			else
			{
				foreach (var movementEventPlayer in m_PlayerInstances)
				{
					movementEventPlayer.Play(movementEventData);
				}	
			}
		}

		protected override float Evaluate(float normalizedSpeed)
		{
			return 1f;
		}
	}
}
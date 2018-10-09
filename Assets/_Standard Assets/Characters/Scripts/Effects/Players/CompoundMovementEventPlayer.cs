using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Plays multiple <see cref="MovementEventPlayer"/> simultaneously
	/// </summary>
	public class CompoundMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField, Tooltip("The prefabs of the movement event players - these are spawned once and played")]
		protected MovementEventPlayer[] playerPrefabs;

		/// <summary>
		/// A cache of the spawned multiple <see cref="MovementEventPlayer"/> instances 
		/// </summary>
		protected MovementEventPlayer[] playerInstances;

		/// <summary>
		/// Initializes, caches and plays the multiple <see cref="MovementEventPlayer"/>
		/// </summary>
		/// <param name="movementEventData">the movement event data model</param>
		protected override void PlayMovementEvent(MovementEventData movementEventData, float effectMagnitude)
		{
			//If the cache does exist or is empty then setup the cache and play the sounds
			if (playerInstances == null || playerInstances.Length == 0)
			{
				playerInstances = new MovementEventPlayer[playerPrefabs.Length];
				int i = -1;
				foreach (MovementEventPlayer movementEventPlayer in playerPrefabs)
				{
					i++;
					MovementEventPlayer instance = Instantiate(movementEventPlayer, transform);
					playerInstances[i] = instance;
					instance.Play(movementEventData);
				}
			}
			//otherwise just play the cached effects
			else
			{
				foreach (MovementEventPlayer movementEventPlayer in playerInstances)
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
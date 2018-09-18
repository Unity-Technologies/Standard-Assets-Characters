using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class CompoundMovementEventPlayer : MovementEventPlayer
	{
		[SerializeField]
		protected MovementEventPlayer[] playerPrefabs;

		protected MovementEventPlayer[] playerInstances;	
		
		protected override void PlayMovementEvent(MovementEventData movementEventData)
		{
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
			else
			{
				foreach (MovementEventPlayer movementEventPlayer in playerInstances)
				{
					movementEventPlayer.Play(movementEventData);
				}	
			}
		}
	}
}
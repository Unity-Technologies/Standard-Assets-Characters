using System;
using System.Collections.Generic;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// A library of movement effects.
	/// This is what would be swapped out for different zones.
	/// e.g. walking on dirt versus walking on metal.
	/// </summary>
	[Serializable]
	public class MovementEventLibrary
	{
		[SerializeField, Tooltip("The movement event player prefab for handling left foot step")]
		protected MovementEventPlayer leftFootStepPrefab;
		
		[SerializeField, Tooltip("The movement event player prefab for handling right foot step")]
		protected MovementEventPlayer rightFootStepPrefab;

		[SerializeField, Tooltip("The movement event player prefab for handling landing")]
		protected MovementEventPlayer landingPrefab;

		[SerializeField, Tooltip("The movement event player prefab for handling jumping")]
		protected MovementEventPlayer jumpingPrefab;

		/// <summary>
		/// Cache of the various instances so that the prefab is only spawned once
		/// </summary>
		protected MovementEventPlayer leftFootStepInstance, rightFootStepInstance, landingInstance, jumpingInstance;

		protected void PlayInstancedEvent(MovementEventData movementEventData, MovementEventPlayer prefab, MovementEventPlayer instance)
		{
			if (prefab == null)
			{
				return;
			}

			if (instance == null)
			{
				instance = GameObject.Instantiate<MovementEventPlayer>(prefab);
			}
			
			instance.Play(movementEventData);
		}

		public void PlayLeftFoot(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, leftFootStepPrefab, leftFootStepInstance);
		}
		
		public void PlayRightFoot(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, rightFootStepPrefab, rightFootStepInstance);
		}
		
		public void PlayLanding(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, landingPrefab, landingInstance);
		}
		
		public void PlayJumping(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, jumpingPrefab, jumpingInstance);
		}
	}
}
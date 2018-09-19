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

		/// <summary>
		/// Helper function for ensuring that the <see cref="MovementEventPlayer"/> prefab is only instantiated once and the cached version is then used
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		/// <param name="prefab">The prefab to instantiate, if it is not cached</param>
		/// <param name="instance">The cached instance of the prefab - this could be null and therefore the keyword ref is required</param>
		protected void PlayInstancedEvent(MovementEventData movementEventData, MovementEventPlayer prefab, ref MovementEventPlayer instance)
		{
			if (prefab == null)
			{
				return;
			}

			if (instance == null)
			{
				instance = GameObject.Instantiate(prefab);
			}
					
			instance.Play(movementEventData);
		}

		/// <summary>
		/// Helper for playing the Left Foot movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayLeftFoot(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, leftFootStepPrefab, ref leftFootStepInstance);
		}
		
		/// <summary>
		/// Helper for playing the Right Foot movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayRightFoot(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, rightFootStepPrefab, ref rightFootStepInstance);
		}
		
		/// <summary>
		/// Helper for playing the Landing movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayLanding(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, landingPrefab, ref landingInstance);
		}
		
		/// <summary>
		/// Helper for playing the Jumping movement event
		/// </summary>
		/// <param name="movementEventData">The data relating to the movement event</param>
		public void PlayJumping(MovementEventData movementEventData)
		{
			PlayInstancedEvent(movementEventData, jumpingPrefab, ref jumpingInstance);
		}
	}
}
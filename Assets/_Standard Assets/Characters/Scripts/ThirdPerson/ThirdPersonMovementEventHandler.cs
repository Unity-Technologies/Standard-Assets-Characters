using System;
using StandardAssets.Characters.Effects;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Handles the third person movement event triggers and event IDs.
	/// As well as collider movement detections <see cref="ColliderMovementDetection"/>
	/// </summary>
	[Serializable]
	public class ThirdPersonMovementEventHandler : MovementEventHandler
	{
		/// <summary>
		/// The movement detection colliders attached to the feet of the Third Person Character
		/// </summary>
		[SerializeField]
		protected ColliderMovementDetection[] movementDetections;

		[SerializeField]
		protected float maximumSpeed = 10f;

		private ThirdPersonBrain thirdPersonBrain;

		/// <summary>
		/// Gives the <see cref="ThirdPersonMovementEventHandler"/> context of the <see cref="ThirdPersonBrain"/>
		/// </summary>
		/// <param name="brainToUse">The <see cref="ThirdPersonBrain"/> that called Init</param>
		public void Init(ThirdPersonBrain brainToUse)
		{
			base.Init(brainToUse);
			thirdPersonBrain = brainToUse;
		}

		/// <summary>
		/// Subscribe to the movement detection events
		/// </summary>
		public void Subscribe()
		{
			foreach (ColliderMovementDetection colliderMovementDetection in movementDetections)
			{
				colliderMovementDetection.detection += HandleMove;
			}
		}

		/// <summary>
		/// Unsubscribe to the movement detection events
		/// </summary>
		public void Unsubscribe()
		{
			foreach (ColliderMovementDetection colliderMovementDetection in movementDetections)
			{
				colliderMovementDetection.detection -= HandleMove;
			}
		}

		public void Jumped()
		{
			//BroadcastMovementEvent(jumpId);
		}

		public void Landed()
		{
		}

		private void HandleMove(MovementEventData movementEventData)
		{
			movementEventData.normalizedSpeed = Mathf.Clamp01(thirdPersonBrain.planarSpeed/maximumSpeed);
			//BroadcastMovementEvent(movementEventData);
		}
	}
}
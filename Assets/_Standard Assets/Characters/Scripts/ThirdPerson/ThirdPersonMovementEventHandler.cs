using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class ThirdPersonMovementEventHandler : MovementEventHandler
	{
		/// <summary>
		/// CharacterPhysics
		/// </summary>
		private ICharacterPhysics characterPhysics;
		
		/// <summary>
		/// The movement detections
		/// </summary>
		[SerializeField]
		protected ColliderMovementDetection[] movementDetections;
		
		/// <summary>
		/// Subscribe to the movement detection events
		/// </summary>
		public void Subscribe()
		{
			foreach (ColliderMovementDetection colliderMovementDetection in movementDetections)
			{
				colliderMovementDetection.detection += BroadcastMovementEvent;
			}
		}
		
		/// <summary>
		/// Subscribe to the movement detection events
		/// </summary>
		public void Unsubscribe()
		{
			foreach (ColliderMovementDetection colliderMovementDetection in movementDetections)
			{
				colliderMovementDetection.detection -= BroadcastMovementEvent;
			}
		}

		public void Jumped()
		{
			BroadcastMovementEvent(jumpId);
		}

		public void Landed()
		{
			BroadcastMovementEvent(landingId);
		}
	}
}
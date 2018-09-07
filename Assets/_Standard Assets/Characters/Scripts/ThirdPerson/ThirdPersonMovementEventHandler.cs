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
		/// Unsubscribe to the movement detection events
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
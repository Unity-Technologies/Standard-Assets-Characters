using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.UI;

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
		/// Id of Jumping event
		/// </summary>
		[SerializeField]
		protected string jumpSoundId = "jumpingSound";

		/// <summary>
		/// Id of Landing event
		/// </summary>
		[SerializeField]
		protected string landSoundId = "landingSound";
		
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
			BroadcastMovementEvent(jumpSoundId);
		}

		public void Landed()
		{
			BroadcastMovementEvent(landSoundId);
		}
	}
}
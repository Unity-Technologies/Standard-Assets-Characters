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

		/*
		 * public void Init(ICharacterPhysics physics)
		{
			//characterPhysics = physics;
		}
		 */
		
	
		
		/// <summary>
		/// Subscribe to the movement detection events
		/// </summary>
		public void Subscribe()
		{
			//characterPhysics.landed += Landed;
			//characterPhysics.jumpVelocitySet += Jumped;
			
			
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
			
			//characterPhysics.landed -= Landed;
			//characterPhysics.jumpVelocitySet -= Jumped;
			
			
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
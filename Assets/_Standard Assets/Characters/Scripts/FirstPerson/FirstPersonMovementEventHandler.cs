using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class FirstPersonMovementEventHandler : MovementEventHandler
	{
		/// <summary>
		/// List of IDs for walking events
		/// </summary>
		[SerializeField]
		protected string[] footIds = new string[]{"leftfoot", "rightfoot"};
		
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
		/// The current index of the 
		/// </summary>
		private int currentIdIndex = -1;

		/// <summary>
		/// Square distance moved from last event and the square of the threshold
		/// </summary>
		private float sqrTravelledDistance, sqrDistanceThreshold;

		/// <summary>
		/// The position that the character was previously	
		/// </summary>
		private Vector3 previousPosition;

		/// <summary>
		/// CharacterPhysics
		/// </summary>
		private ICharacterPhysics characterPhysics;

		private Transform transform;
		
		/// <summary>
		/// Initialize:
		/// Precalculate the square of the threshold
		/// Set the previous position
		/// </summary>
		public void Init(Transform newTransform, ICharacterPhysics physics)
		{
			base.Init();
			transform = newTransform;
			previousPosition = transform.position;
			characterPhysics = physics;
		}

		public void Tick()
		{
			Vector3 currentPosition = transform.position;
			
			//Optimization - prevents the rest of the logic, which includes vector magnitude calculations, from being called if the character has not moved
			if (currentPosition == previousPosition || !characterPhysics.isGrounded)
			{
				previousPosition = currentPosition;
				return;
			}
			
			sqrTravelledDistance += (currentPosition - previousPosition).sqrMagnitude;

			if (sqrTravelledDistance >= sqrDistanceThreshold)
			{
				sqrTravelledDistance = 0;
				Moved();
			}
			
			previousPosition = currentPosition;
		}
		
		/// <summary>
		/// Subscribe
		/// </summary>
		public void Subscribe()
		{
			characterPhysics.landed += Landed;
			characterPhysics.jumpVelocitySet += Jumped;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		public void Unsubscribe()
		{
			characterPhysics.landed -= Landed;
			characterPhysics.jumpVelocitySet -= Jumped;
		}    
		
		/// <summary>
		/// Handle the broadcasting of the movement event
		/// </summary>
		private void Moved()
		{
			int length = footIds.Length;
			if (footIds == null || length == 0)
			{
				return;
			}

			currentIdIndex++;
			if (currentIdIndex >= length)
			{
				currentIdIndex = 0;
			}

			BroadcastMovementEvent(footIds[currentIdIndex]);
		}
		
		/// <summary>
		/// Calls PlayEvent on the jump ID
		/// </summary>
		void Jumped()
		{
			BroadcastMovementEvent(jumpSoundId);
		}
		
		/// <summary>
		/// Calls PlayEvent on the landing ID
		/// </summary>
		void Landed()
		{
			BroadcastMovementEvent(landSoundId);
		}
		
		/// <summary>
		/// Change the distance that footstep sounds are played
		/// </summary>
		/// <param name="strideLength"></param>
		public void AdjustAudioTriggerThreshold(float strideLength)
		{		
			sqrDistanceThreshold = strideLength * strideLength;
		}
	}
}
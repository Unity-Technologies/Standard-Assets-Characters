using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	[Serializable]
	public class CapsuleMovementEventHandler : DistanceMovementEventHandler
	{
		/// <summary>
		/// Distance travelled between movement events
		/// </summary>
		[SerializeField]
		protected float walkDistanceThreshold = 1f;

		/// <summary>
		/// Initialize:
		/// Precalculate the square of the threshold
		/// Set the previous position
		/// </summary>
		public void Init(CapsuleBrain brainToUse, Transform newTransform, ICharacterPhysics physics)
		{
			base.Init(brain);
			sqrDistanceThreshold = walkDistanceThreshold * walkDistanceThreshold;
			transform = newTransform;
			previousPosition = transform.position;
		}
		
		/// <summary>
		/// Subscribe
		/// </summary>
		public void Subscribe()
		{
			brain.physicsForCharacter.landed += Landed;
			brain.physicsForCharacter.jumpVelocitySet += Jumped;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		public void Unsubscribe()
		{
			brain.physicsForCharacter.landed -= Landed;
			brain.physicsForCharacter.jumpVelocitySet -= Jumped;
		}    
		
	
		/// <summary>
		/// Calls PlayEvent on the jump ID
		/// </summary>
		void Jumped()
		{
			//BroadcastMovementEvent(jumpId);
		}
		
		/// <summary>
		/// Calls PlayEvent on the landing ID
		/// </summary>
		void Landed()
		{
			//BroadcastMovementEvent(landingId);
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
using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class FirstPersonMovementEventHandler : DistanceMovementEventHandler
	{
		/// <summary>
		/// Initialize:
		/// Precalculate the square of the threshold
		/// Set the previous position
		/// </summary>
		public void Init(FirstPersonBrain brainToUse)
		{
			base.Init(brainToUse);
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
		private void Jumped()
		{
			//BroadcastMovementEvent(jumpId, transform);
		}

		/// <summary>
		/// Calls PlayEvent on the landing ID
		/// </summary>
		private void Landed()
		{
			//BroadcastMovementEvent(landingId, transform);
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
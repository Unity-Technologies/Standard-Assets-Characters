using System;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	/// <summary>
	/// <see cref="MovementEventHandler"/> for the Capsule Character example 
	/// </summary>
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
		public void Init(CapsuleBrain brainToUse, Transform newTransform, CharacterPhysics physics)
		{
			base.Init(brainToUse);
			sqrDistanceThreshold = walkDistanceThreshold * walkDistanceThreshold;
			transform = newTransform;
			previousPosition = transform.position;
		}
		
		/// <summary>
		/// Change the distance that footstep sounds are played
		/// </summary>
		/// <param name="strideLength">the length of a stride for a specific movement type</param>
		public void AdjustAudioTriggerThreshold(float strideLength)
		{
			sqrDistanceThreshold = strideLength * strideLength;
		}
	}
}
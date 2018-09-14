using System;
using Cinemachine;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class FirstPersonMovementEventHandler : MovementEventHandler
	{
		[SerializeField, Tooltip("The maximum speed of the character")]
		protected float maximumSpeed = 10f;
		
		/// <summary>
		/// List of IDs for walking events
		/// </summary>
		[SerializeField]
		protected string[] footIds = new string[]{"leftfoot", "rightfoot"};

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
		/// Transform used for tracking distance
		/// </summary>
		private Transform transform;

		/// <summary>
		/// The character brain for used in speed scaling
		/// </summary>
		private FirstPersonBrain brain;
		
		/// <summary>
		/// Initialize:
		/// Precalculate the square of the threshold
		/// Set the previous position
		/// </summary>
		public void Init(FirstPersonBrain brainToUse)
		{
			base.Init();
			transform = brainToUse.transform;
			previousPosition = transform.position;
			brain = brainToUse;
		}

		public void Tick()
		{
			Vector3 currentPosition = transform.position;
			
			//Optimization - prevents the rest of the logic, which includes vector magnitude calculations, from being called if the character has not moved
			if (currentPosition == previousPosition || !brain.physicsForCharacter.isGrounded)
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

			BroadcastMovementEvent(footIds[currentIdIndex], transform, Mathf.Clamp01(brain.planarSpeed/maximumSpeed));
		}
		
		/// <summary>
		/// Calls PlayEvent on the jump ID
		/// </summary>
		private void Jumped()
		{
			BroadcastMovementEvent(jumpId, transform);
		}
		
		/// <summary>
		/// Calls PlayEvent on the landing ID
		/// </summary>
		private void Landed()
		{
			BroadcastMovementEvent(landingId, transform);
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
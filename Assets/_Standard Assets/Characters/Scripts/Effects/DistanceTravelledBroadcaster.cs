using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Broadcasts an event with an id selected from a list every x units of movementg
	/// </summary>
	[RequireComponent(typeof(ICharacterPhysics))]
	public class DistanceTravelledBroadcaster : MovementEventBroadcaster
	{
		/// <summary>
		/// Distance travelled between movement events
		/// </summary>
		[SerializeField]
		protected float distanceThreshold = 1f;

		/// <summary>
		/// List of IDs for movement events
		/// </summary>
		[SerializeField]
		protected string[] ids;

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

		/// <summary>
		/// Initialize:
		/// Precalculate the square of the threshold
		/// Set the previous position
		/// </summary>
		private void Awake()
		{
			sqrDistanceThreshold = distanceThreshold * distanceThreshold;
			previousPosition = transform.position;
			characterPhysics = GetComponent<ICharacterPhysics>();
		}

		
		/// <summary>
		/// Calculate movement on FixedUpdate
		/// </summary>
		private void FixedUpdate()
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
		/// Handle the broadcasting of the movement event
		/// </summary>
		private void Moved()
		{
			int length = ids.Length;
			if (ids == null || length == 0)
			{
				return;
			}

			currentIdIndex++;
			if (currentIdIndex >= length)
			{
				currentIdIndex = 0;
			}

			BroadcastMovementEvent(ids[currentIdIndex]);
		}
	}
}
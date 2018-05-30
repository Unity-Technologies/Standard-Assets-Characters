using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Broadcasts an event with an id selected from a list every x units of movementg
	/// </summary>
	public class DistanceTravelledBroadcaster : MovementEventBroadcaster
	{
		/// <summary>
		/// Distance travelled between movement events
		/// </summary>
		public float distanceThreshold = 1f;

		/// <summary>
		/// List of IDs for movement events
		/// </summary>
		public string[] ids;

		/// <summary>
		/// The current index of the 
		/// </summary>
		int m_CurrentIdIndex = -1;

		/// <summary>
		/// Square distance moved from last event and the square of the threshold
		/// </summary>
		float m_SqrTravelledDistance = 0f, m_SqrDistanceThreshold;

		/// <summary>
		/// The position that the character was previously	
		/// </summary>
		Vector3 m_PreviousPosition;

		/// <summary>
		/// Initialize:
		/// Precalculate the square of the threshold
		/// Set the previous position
		/// </summary>
		void Awake()
		{
			m_SqrDistanceThreshold = distanceThreshold * distanceThreshold;
			m_PreviousPosition = transform.position;
		}

		/// <summary>
		/// Calculate movement on FixedUpdate
		/// </summary>
		void FixedUpdate()
		{
			Vector3 currentPosition = transform.position;
			
			//Optimization - prevents the rest of the logic, which includes vector magnitude calculations, from being called if the character has not moved
			if (currentPosition == m_PreviousPosition)
			{
				return;
			}
			
			m_SqrTravelledDistance += (currentPosition - m_PreviousPosition).sqrMagnitude;

			if (m_SqrTravelledDistance >= m_SqrDistanceThreshold)
			{
				m_SqrTravelledDistance = 0;
				Moved();
			}
			
			m_PreviousPosition = currentPosition;
		}

		/// <summary>
		/// Handle the broadcasting of the movement event
		/// </summary>
		void Moved()
		{
			int length = ids.Length;
			if (ids == null || length == 0)
			{
				return;
			}

			m_CurrentIdIndex++;
			if (m_CurrentIdIndex >= length)
			{
				m_CurrentIdIndex = 0;
			}

			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = ids[m_CurrentIdIndex];
			
			OnMoved(movementEvent);
		}
	}
}
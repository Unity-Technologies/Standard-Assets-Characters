using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class DistanceTravelledBroadcaster : MovementEventBroadcaster
	{
		public float distanceThreshold = 1f;

		public string[] ids;

		int m_CurrentIdIndex = -1;

		float m_SqrTravelledDistance = 0f, m_SqrDistanceThreshold;

		Vector3 m_PreviousPosition;

		void Awake()
		{
			m_SqrDistanceThreshold = distanceThreshold * distanceThreshold;
			m_PreviousPosition = transform.position;
		}

		void FixedUpdate()
		{
			Vector3 currentPosition = transform.position;
			m_SqrTravelledDistance += (currentPosition - m_PreviousPosition).sqrMagnitude;

			if (m_SqrTravelledDistance >= m_SqrDistanceThreshold)
			{
				m_SqrTravelledDistance = 0;
				Moved();
			}
			
			m_PreviousPosition = currentPosition;
		}

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
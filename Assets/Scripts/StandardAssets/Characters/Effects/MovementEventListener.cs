using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Listens to MovementEvents from MovementEventBroadcasters
	/// </summary>
	public class MovementEventListener : MonoBehaviour
	{
		public MovementEventLibrary startingMovementEventLibrary;
		
		/// <summary>
		/// The attached movement event broadcasters
		/// </summary>
		MovementEventBroadcaster[] m_Broadcasters;
		
		/// <summary>
		/// The current movement event library
		/// </summary>
		MovementEventLibrary m_MovementEventLibrary;

		/// <summary>
		/// Sets the current movement event library
		/// </summary>
		/// <param name="movementEventLibrary"></param>
		public void SetCurrentMovementEventLibrary(MovementEventLibrary movementEventLibrary)
		{
			m_MovementEventLibrary = movementEventLibrary;
		}

		/// <summary>
		/// Set the starting movement library
		/// </summary>
		void Awake()
		{
			m_MovementEventLibrary = startingMovementEventLibrary;
		}

		/// <summary>
		/// Subscribe to the broadcasters
		/// </summary>
		void OnEnable()
		{
			if (m_Broadcasters == null || m_Broadcasters.Length == 0)
			{
				m_Broadcasters = GetComponentsInChildren<MovementEventBroadcaster>();
			}
			
			foreach (MovementEventBroadcaster broadcaster in m_Broadcasters)
			{
				broadcaster.moved += OnMoved;
			}
		}
		
		/// <summary>
		/// Unsubscribe to the broadcasters
		/// </summary>
		void OnDisable()
		{
			foreach (MovementEventBroadcaster broadcaster in m_Broadcasters)
			{
				broadcaster.moved -= OnMoved;
			}
		}

		/// <summary>
		/// Plays the movement event
		/// </summary>
		/// <param name="movementEvent"></param>
		void OnMoved(MovementEvent movementEvent)
		{
			if (m_MovementEventLibrary == null)
			{
				return;
			}
			
			m_MovementEventLibrary.PlayEvent(movementEvent);
		}
	}
}
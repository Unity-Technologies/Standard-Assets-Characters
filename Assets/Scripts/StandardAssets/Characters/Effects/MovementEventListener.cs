using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Listens to MovementEvents from MovementEventBroadcasters
	/// </summary>
	public class MovementEventListener : MonoBehaviour
	{
		[SerializeField]
		protected MovementEventLibrary startingMovementEventLibrary;
		
		/// <summary>
		/// The attached movement event broadcasters
		/// </summary>
		private MovementEventBroadcaster[] broadcasters;
		
		/// <summary>
		/// The current movement event library
		/// </summary>
		private MovementEventLibrary movementEventLibrary;

		/// <summary>
		/// Sets the current movement event library
		/// </summary>
		/// <param name="newMovementEventLibrary"></param>
		public void SetCurrentMovementEventLibrary(MovementEventLibrary newMovementEventLibrary)
		{
			movementEventLibrary = newMovementEventLibrary;
		}

		/// <summary>
		/// Set the starting movement library
		/// </summary>
		private void Awake()
		{
			movementEventLibrary = startingMovementEventLibrary;
		}

		/// <summary>
		/// Subscribe to the broadcasters
		/// </summary>
		private void OnEnable()
		{
			if (broadcasters == null || broadcasters.Length == 0)
			{
				broadcasters = GetComponentsInChildren<MovementEventBroadcaster>();
			}
			
			foreach (MovementEventBroadcaster broadcaster in broadcasters)
			{
				broadcaster.moved += OnMoved;
			}
		}
		
		/// <summary>
		/// Unsubscribe to the broadcasters
		/// </summary>
		private void OnDisable()
		{
			foreach (MovementEventBroadcaster broadcaster in broadcasters)
			{
				broadcaster.moved -= OnMoved;
			}
		}

		/// <summary>
		/// Plays the movement event
		/// </summary>
		/// <param name="movementEvent"></param>
		private void OnMoved(MovementEvent movementEvent)
		{
			if (movementEventLibrary == null)
			{
				return;
			}
			
			movementEventLibrary.PlayEvent(movementEvent);
		}
	}
}
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Listens to MovementEvents from MovementEventBroadcasters
	/// </summary>
	public class MovementEventListener : MonoBehaviour
	{
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
	}
}
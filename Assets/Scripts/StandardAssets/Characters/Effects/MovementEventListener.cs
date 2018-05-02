using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public class MovementEventListener : MonoBehaviour
	{
		MovementEventLibrary m_MovementEventLibrary;

		public void SetCurrentMovementEventLibrary(MovementEventLibrary movementEventLibrary)
		{
			m_MovementEventLibrary = movementEventLibrary;
		}
	}
}
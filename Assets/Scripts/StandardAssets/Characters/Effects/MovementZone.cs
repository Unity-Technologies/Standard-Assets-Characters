using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// An abstract representation of MovementZone
	/// </summary>
	public abstract class MovementZone : MonoBehaviour
	{
		/// <summary>
		/// The library of events to be played 
		/// </summary>
		[SerializeField]
		protected MovementEventLibrary library;

		/// <summary>
		/// Helper method for triggering movement events
		/// </summary>
		/// <param name="handler"></param>
		protected void Trigger(MovementEventHandler handler)
		{
			if (handler == null)
			{
				return;
			}
			
			handler.SetCurrentMovementEventLibrary(library);
		}
	}
}
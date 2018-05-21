using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <inheritdoc />
	[RequireComponent(typeof(Collider))]
	public class TriggerMovementZone : MovementZone
	{
		/// <summary>
		/// Change the movement event library on trigger enter
		/// </summary>
		/// <param name="other"></param>
		void OnTriggerEnter(Collider other)
		{
			MovementEventListener listener = other.GetComponent<MovementEventListener>();
			if (listener != null)
			{
				Trigger(listener);
			}
		}
	}
}
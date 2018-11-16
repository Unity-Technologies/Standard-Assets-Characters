using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Detect collisions or triggers and broadcast <see cref="MovementEventData"/>
	/// e.g. BoxColliders on the feet for ThirdPerson character
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class ColliderMovementDetection : MonoBehaviour
	{
		[SerializeField, Tooltip("Layer that will trigger the broadcast of this movement event handler ID")]
		LayerMask m_LayerMask;
		
		/// <summary>
		/// Fired when movement is detected
		/// </summary>
		public event Action<MovementEventData, PhysicMaterial> detection;

		void OnTriggerEnter(Collider other)
		{
			if (m_LayerMask != (m_LayerMask | (1 << other.gameObject.layer)))
			{
				return;
			}

			var movementEventData = new MovementEventData(transform);
			OnDetection(movementEventData, other.sharedMaterial);			
		}

		/// <summary>
		/// Safely broadcast movement event after collider detection
		/// </summary>
		/// <param name="movementEventData">Movement event data</param>
		/// <param name="physicMaterial">the corresponding physic material</param>
		void OnDetection(MovementEventData movementEventData, PhysicMaterial physicMaterial)
		{
			if (detection == null)
			{
				return;
			}
			
			detection(movementEventData, physicMaterial);
		}
	}
}
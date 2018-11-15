using System;
using UnityEngine;
using UnityEngine.Serialization;

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
		public event Action<MovementEventData> detection;

		bool m_IsTrigger;

		void Awake()
		{
			m_IsTrigger = GetComponent<Collider>().isTrigger;
		}

		void OnTriggerEnter(Collider other)
		{
			
			if (!m_IsTrigger)
			{
				return;
			}

			if (m_LayerMask != (m_LayerMask | (1 << other.gameObject.layer)))
			{
				return;
			}

			var movementEventData = new MovementEventData(transform);
			OnDetection(movementEventData);			
		}

		void OnCollisionEnter(Collision other)
		{
			if (m_IsTrigger)
			{
				return;
			}
			
			if (m_LayerMask != (m_LayerMask | (1 << other.gameObject.layer)))
			{
				return;
			}

			var movementEventData = new MovementEventData(transform);
			OnDetection(movementEventData);
		}

		/// <summary>
		/// Safely broadcast movement event after collider detection
		/// </summary>
		/// <param name="movementEventData">Movement event data</param>
		void OnDetection(MovementEventData movementEventData)
		{
			if (detection == null)
			{
				return;
			}
			
			detection(movementEventData);
		}
	}
}
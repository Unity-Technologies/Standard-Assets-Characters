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
		[SerializeField, Tooltip("The layer that will trigger the broadcast of this movement event handler ID")]
		protected LayerMask layerMask;
		
		/// <summary>
		/// Fired when movement is detected
		/// </summary>
		public event Action<MovementEventData> detection;

		private bool isTrigger;
		
		private void Awake()
		{
			isTrigger = GetComponent<Collider>().isTrigger;
		}

		private void OnTriggerEnter(Collider other)
		{
			
			if (!isTrigger)
			{
				return;
			}

			if (layerMask != (layerMask | (1 << other.gameObject.layer)))
			{
				return;
			}

			MovementEventData movementEventData = new MovementEventData();
			movementEventData.firedFrom = transform;
			OnDetection(movementEventData);			
		}
		
		private void OnCollisionEnter(Collision other)
		{
			if (isTrigger)
			{
				return;
			}
			
			if (layerMask != (layerMask | (1 << other.gameObject.layer)))
			{
				return;
			}
			
			MovementEventData movementEventData = new MovementEventData();
			movementEventData.firedFrom = transform;
			OnDetection(movementEventData);
		}

		/// <summary>
		/// Safely broadcast movement event after collider detection
		/// </summary>
		/// <param name="movementEventData">Movement event data</param>
		private void OnDetection(MovementEventData movementEventData)
		{
			if (detection == null)
			{
				return;
			}
			
			detection(movementEventData);
		}
	}
}
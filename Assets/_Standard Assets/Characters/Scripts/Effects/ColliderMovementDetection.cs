using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Detect collisions or triggers and broadcast <see cref="MovementEvent"/>
	/// e.g. BoxColliders on the feet for ThirdPerson character
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class ColliderMovementDetection : MonoBehaviour
	{
		/// <summary>
		/// The movement event ID corresponding to the <see cref="MovementEventHandler"/>
		/// </summary>
		[SerializeField]
		protected string id;

		[SerializeField, Tooltip("The layer that will trigger the broadcast of this movement event handler ID")]
		protected LayerMask layerMask;
		
		/// <summary>
		/// Fired when movement is detected
		/// </summary>
		public event Action<MovementEvent> detection;

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

			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = id;
			movementEvent.firedFrom = transform;
			//TODO set position
			OnDetection(movementEvent);			
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
			
			MovementEvent movementEvent = new MovementEvent();
			movementEvent.id = id;
			movementEvent.firedFrom = transform;
			//TODO set position
			OnDetection(movementEvent);
		}

		/// <summary>
		/// Safely broadcast movement event after collider detection
		/// </summary>
		/// <param name="movementEvent">Movement event data</param>
		private void OnDetection(MovementEvent movementEvent)
		{
			detection(movementEvent);
		}
	}
}
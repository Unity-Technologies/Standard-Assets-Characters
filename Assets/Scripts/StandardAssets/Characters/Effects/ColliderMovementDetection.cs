using System;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Used in conjunction with Colliders to detect movement
	/// e.g. BoxColliders on the feet for ThirdPerson character
	/// </summary>
	[RequireComponent(typeof(Collider))]
	public class ColliderMovementDetection : MonoBehaviour
	{
		/// <summary>
		/// The movement event id
		/// </summary>
		public string id;

		public LayerMask layerMask;
		
		/// <summary>
		/// Fired when movement is detected
		/// </summary>
		public event Action<MovementEvent> detection;

		/// <summary>
		/// Whether or not the attached collider is a trigger
		/// </summary>
		private bool isTrigger;

		private void Awake()
		{
			//Is the Collider a Trigger
			isTrigger = GetComponent<Collider>().isTrigger;
		}

		/// <summary>
		/// Handle triggering
		/// </summary>
		/// <param name="other"></param>
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

		/// <summary>
		/// Handle colliding
		/// </summary>
		/// <param name="other"></param>
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
		/// Safely broadcast detection
		/// </summary>
		/// <param name="movementEvent"></param>
		private void OnDetection(MovementEvent movementEvent)
		{
			if (detection != null)
			{
				detection(movementEvent);
			}
		}
	}
}
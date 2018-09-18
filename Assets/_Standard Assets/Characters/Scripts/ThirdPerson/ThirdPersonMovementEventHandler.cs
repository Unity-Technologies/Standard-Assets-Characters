using System;
using StandardAssets.Characters.Effects;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Handles the third person movement event triggers and event IDs.
	/// As well as collider movement detections <see cref="ColliderMovementDetection"/>
	/// </summary>
	[Serializable]
	public class ThirdPersonMovementEventHandler : MovementEventHandler
	{
		/// <summary>
		/// The movement detection colliders attached to the feet of the Third Person Character
		/// </summary>
		[SerializeField]
		protected ColliderMovementDetection leftFootDetection;
		
		[SerializeField]
		protected ColliderMovementDetection rightFootDetection;

		[SerializeField]
		protected float maximumSpeed = 10f;

		private ThirdPersonBrain thirdPersonBrain;

		/// <summary>
		/// Gives the <see cref="ThirdPersonMovementEventHandler"/> context of the <see cref="ThirdPersonBrain"/>
		/// </summary>
		/// <param name="brainToUse">The <see cref="ThirdPersonBrain"/> that called Init</param>
		public void Init(ThirdPersonBrain brainToUse)
		{
			base.Init(brainToUse);
			thirdPersonBrain = brainToUse;
		}

		/// <summary>
		/// Subscribe to the movement detection events
		/// </summary>
		public void Subscribe()
		{
			leftFootDetection.detection += HandleLeftFoot;
			rightFootDetection.detection += HandleRightFoot;
		}

		/// <summary>
		/// Unsubscribe to the movement detection events
		/// </summary>
		public void Unsubscribe()
		{
			leftFootDetection.detection -= HandleLeftFoot;
			rightFootDetection.detection -= HandleRightFoot;
		}

		public void Jumped()
		{
			PlayJumping(new MovementEventData(brain.transform));
		}

		public void Landed()
		{
			PlayLanding(new MovementEventData(brain.transform));
		}

		private void HandleLeftFoot(MovementEventData movementEventData)
		{
			movementEventData.normalizedSpeed = Mathf.Clamp01(thirdPersonBrain.planarSpeed/maximumSpeed);
			PlayLeftFoot(movementEventData);
		}
		
		private void HandleRightFoot(MovementEventData movementEventData)
		{
			movementEventData.normalizedSpeed = Mathf.Clamp01(thirdPersonBrain.planarSpeed/maximumSpeed);
			PlayRightFoot(movementEventData);
		}
	}
}
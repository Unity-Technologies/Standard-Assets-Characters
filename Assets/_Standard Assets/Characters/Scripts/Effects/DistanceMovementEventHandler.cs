using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Abstract base for all movement event handlers that use distance travelled while grounded to broadcast left and right footsteps
	/// </summary>
	public abstract class DistanceMovementEventHandler : MovementEventHandler
	{
		[SerializeField, Tooltip("The maximum speed of the character")]
		protected float maximumSpeed = 10f;

		protected float sqrTravelledDistance;

		protected float sqrDistanceThreshold;

		protected Vector3 previousPosition;

		protected bool isLeftFoot;

		protected Transform transform;

		/// <summary>
		/// Initializes the handler with the correct character brain and sets up the transform and previousPosition needed to calculate distance travelled
		/// </summary>
		/// <param name="brainToUse"></param>
		public override void Init(CharacterBrain brainToUse)
		{
			base.Init(brainToUse);
			transform = brainToUse.transform;
			previousPosition = transform.position;
		}

		/// <summary>
		/// Updates the distance travelled and checks if footstep events need to be fired
		/// </summary>
		public void Tick()
		{
			Vector3 currentPosition = transform.position;

			//If the character has not moved or is not grounded then ignore the calculations that follow
			if (currentPosition == previousPosition || !brain.physicsForCharacter.isGrounded)
			{
				previousPosition = currentPosition;
				return;
			}

			sqrTravelledDistance += (currentPosition - previousPosition).sqrMagnitude;

			if (sqrTravelledDistance >= sqrDistanceThreshold)
			{
				sqrTravelledDistance = 0;
				MovementEventData data =
					new MovementEventData(transform, Mathf.Clamp01(brain.planarSpeed / maximumSpeed));
				if (isLeftFoot)
				{
					PlayLeftFoot(data);
				}
				else
				{
					PlayRightFoot(data);
				}

				isLeftFoot = !isLeftFoot;
			}

			previousPosition = currentPosition;
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		public void Subscribe()
		{
			if (brain == null || brain.physicsForCharacter == null)
			{
				return;
			}
			brain.physicsForCharacter.landed += Landed;
			brain.physicsForCharacter.jumpVelocitySet += Jumped;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		public void Unsubscribe()
		{
			if (brain == null || brain.physicsForCharacter == null)
			{
				return;
			}
			brain.physicsForCharacter.landed -= Landed;
			brain.physicsForCharacter.jumpVelocitySet -= Jumped;
		}

		/// <summary>
		/// Calls PlayEvent on the jump ID
		/// </summary>
		private void Jumped()
		{
			PlayJumping(new MovementEventData(transform));
		}

		/// <summary>
		/// Calls PlayEvent on the landing ID
		/// </summary>
		private void Landed()
		{
			PlayLanding(new MovementEventData(transform));
		}
	}
}
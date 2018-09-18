using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Effects
{
	public abstract class DistanceMovementEventHandler : MovementEventHandler
	{
		[SerializeField, Tooltip("The maximum speed of the character")]
		protected float maximumSpeed = 10f;

		protected float sqrTravelledDistance;
		
		protected float sqrDistanceThreshold;
		

		protected Vector3 previousPosition;

		protected bool isLeftFoot;

		protected Transform transform;

		public override void Init(CharacterBrain brainToUse)
		{
			base.Init(brainToUse);
			transform = brainToUse.transform;
			previousPosition = transform.position;
		}
		
		public void Tick()
		{
			Vector3 currentPosition = transform.position;

			//Optimization - prevents the rest of the logic, which includes vector magnitude calculations, from being called if the character has not moved
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
	}
}
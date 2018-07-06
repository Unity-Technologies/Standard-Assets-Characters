using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonMotorProperties
	{
		[SerializeField]
		protected float jumpSpeed;
		
		[SerializeField]
		protected AnimationInputProperties forwardMovement;

		[SerializeField]
		protected AnimationInputProperties strafeForwardMovement;

		[SerializeField]
		protected AnimationInputProperties strafeLateralMovement;
		
		public AnimationInputProperties forwardMovementProperties
		{
			get { return forwardMovement; }
		}

		public AnimationInputProperties strafeForwardMovementProperties
		{
			get { return strafeForwardMovement; }
		}

		public AnimationInputProperties strafeLateralMovementProperties
		{
			get { return strafeLateralMovement; }
		}

		public float initialJumpVelocity
		{
			get { return jumpSpeed; }
		}
	}
}
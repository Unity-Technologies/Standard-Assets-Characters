using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[CreateAssetMenu(fileName = "Third Person Configuration", menuName = "Standard Assets/Characters/Create Third Person Configuration", order = 1)]
	public class ThirdPersonConfiguration : ScriptableObject
	{
		[SerializeField]
		protected float jumpSpeed = 10f;
		
		[SerializeField]
		protected float turningSpeed = 500f;
		
		[SerializeField]
		protected float strafeLookInputScale = 20f;

		[SerializeField]
		protected float rootMotionMovementFactor = 1f;
		
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

		public float turningLerp
		{
			get { return turningSpeed; }
		}

		public float scaleRootMovement
		{
			get { return rootMotionMovementFactor; }
		}

		public float scaleStrafeLook
		{
			get { return strafeLookInputScale; }
		}
	}
}
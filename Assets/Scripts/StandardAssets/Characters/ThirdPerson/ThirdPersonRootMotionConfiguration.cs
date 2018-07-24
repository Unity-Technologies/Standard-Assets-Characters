using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[CreateAssetMenu(fileName = "Third Person Root Motion Configuration", menuName = "Standard Assets/Characters/Third Person Root Motion Configuration", order = 1)]
	public class ThirdPersonRootMotionConfiguration : ScriptableObject
	{
		[SerializeField]
		protected float jumpSpeed = 10f;

		[SerializeField]
		protected float jumpGroundVelocityScale = 1f;
		
		[SerializeField]
		protected float turningSpeed = 500f;

		[SerializeField]
		protected float turningSpeedVisualScale = 0.5f;

		[SerializeField]
		protected float jumpTurningSpeedScale = 0.5f;

		[SerializeField]
		protected float turningLerp = 1f;
		
		[SerializeField]
		protected float strafeLookInputScale = 20f;

		[SerializeField]
		protected float rootMotionMovementScale = 1f;

		[SerializeField]
		protected float rapidTurnAngle = 140f;
		
		[SerializeField]
		protected AnimationInputProperties forwardMovement;

		[SerializeField]
		protected AnimationInputProperties strafeForwardMovement;

		[SerializeField]
		protected AnimationInputProperties strafeLateralMovement;
		
		[SerializeField, Tooltip("A fall distance higher than this will trigger a fall animation")]
		protected float maxFallDistance = 1;
		
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

		public float scaledGroundVelocity
		{
			get { return jumpGroundVelocityScale; }
		}

		public float turningYSpeed
		{
			get { return turningSpeed; }
		}

		public float turningSpeedScaleVisual
		{
			get { return turningSpeedVisualScale; }
		}
		
		public float jumpTurningYSpeed
		{
			get { return turningSpeed * jumpTurningSpeedScale; }
		}

		public float scaleRootMovement
		{
			get { return rootMotionMovementScale; }
		}

		public float scaleStrafeLook
		{
			get { return strafeLookInputScale; }
		}

		public float turningLerpFactor
		{
			get { return turningLerp; }
		}

		public float angleRapidTurn
		{
			get { return rapidTurnAngle; }
		}

		public float maxFallDistanceToLand
		{
			get { return maxFallDistance; }
		}
	}
}
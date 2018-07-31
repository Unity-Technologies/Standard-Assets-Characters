using Attributes;
using Attributes.Types;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[CreateAssetMenu(fileName = "Third Person Root Motion Configuration", menuName = "Standard Assets/Characters/Third Person Root Motion Configuration", order = 1)]
	public class ThirdPersonRootMotionConfiguration : ScriptableObject
	{
		[Header("Ground Motion")]
		[SerializeField]
		protected float rootMotionMovementScale = 1f;

		[SerializeField]
		protected bool useCustomStrafeParameters = true;

		[ConditionalInclude("useCustomStrafeParameters")]
		[SerializeField]
		protected StrafeProperties strafing;
		
		[Header("Jumping")]
		[SerializeField]
		protected float jumpSpeed = 10f;
		[SerializeField]
		protected int jumpGroundVelocitySamples = 1;
		[SerializeField]
		protected float jumpGroundVelocityScale = 1f;
		[SerializeField]
		protected float jumpTurningSpeedScale = 0.5f;
		
		[Header("Turning")]
		[SerializeField]
		protected float turningSpeed = 500f;
		[SerializeField]
		protected float turningSpeedVisualScale = 0.5f;
		[SerializeField]
		protected float turningLerp = 1f;
		[SerializeField]
		protected float rapidTurnAngle = 140f;

		[Header("Camera")]
		[SerializeField]
		protected float strafeLookInputScale = 20f;

		[SerializeField]
		protected AnimationInputProperties forwardMovement;

		
		[SerializeField, Tooltip("A fall distance higher than this will trigger a fall animation")]
		protected float maxFallDistance = 1;
		
		public AnimationInputProperties forwardMovementProperties
		{
			get { return forwardMovement; }
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

		public int jumpGroundVelocityWindowSize
		{
			get { return jumpGroundVelocitySamples; }
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

		public float normalizedForwardStrafeSpeed
		{
			get { return useCustomStrafeParameters ? strafing.normalizedForwardStrafeSpeed : 1f; }
		}

		public float normalizedBackwardStrafeSpeed
		{
			get { return useCustomStrafeParameters ? strafing.normalizedBackwardStrafeSpeed : 1f; }
		}

		public float normalizedLateralStrafeSpeed
		{
			get { return useCustomStrafeParameters ? strafing.normalizedLateralStrafeSpeed : 1f; }
		}
		
		public int strafeInputWindowSize
		{
			get { return useCustomStrafeParameters ? strafing.strafeInputWindowSize : 1; }
		}
		
	}
}
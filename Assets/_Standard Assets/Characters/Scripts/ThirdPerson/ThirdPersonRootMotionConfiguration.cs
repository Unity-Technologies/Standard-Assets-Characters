using StandardAssets.Characters.Attributes;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[CreateAssetMenu(fileName = "Third Person Root Motion Configuration",
		menuName = "Standard Assets/Characters/Third Person Root Motion Configuration", order = 1)]
	public class ThirdPersonRootMotionConfiguration : ScriptableObject
	{
		[Header("Ground Motion")]
		[SerializeField]
		protected float rootMotionMovementScale = 1f;

		[SerializeField]
		protected bool useCustomActionParameters = true;

		[VisibleIf("useCustomActionParameters")]
		[SerializeField]
		protected ActionProperties action;

		[SerializeField]
		protected bool useCustomStrafeParameters = true;
		
		[SerializeField]
		protected float sprintNormalizedSpeedIncrease = 0.5f;

		[VisibleIf("useCustomStrafeParameters")]
		[SerializeField]
		protected StrafeProperties strafing;

		[Header("Jumping")]
		[SerializeField]
		protected AnimationCurve jumpHeightAsAFactorOfForwardSpeed = AnimationCurve.Constant(0,1,4);

		[SerializeField]
		protected int jumpGroundVelocitySamples = 1;

		[SerializeField]
		protected float jumpGroundVelocityScale = 1f;

		[SerializeField]
		protected float jumpTurningSpeedScale = 0.5f;
		
		[SerializeField]
		protected int postPhyicsJumpFramesToIgnoreForwardSpeed = 10;

		[Header("Standing Jump")]
		[SerializeField]
		protected float standingJumpForwardSpeed = 0.1f;
		[SerializeField]
		protected float minInputThreshold = 0.5f,
						maxMovementThreshold = 0.01f;

		[Header("Falling")]
		[SerializeField]
		protected float fallingMaxForwardSpeed = 5;

		[SerializeField]
		protected float fallForwardSpeedDeceleration = 0.0025f;
		
		[SerializeField]
		protected float fallForwardSpeedAcceleration = 0.05f;
		
		[SerializeField, Tooltip("A fall distance higher than this will trigger a fall animation")]
		protected float maxFallDistance = 1;

		[SerializeField] 
		protected float fallDirectionChangeSpeed = 0.025f;

		[Header("Turning")]
		[SerializeField]
		protected float turningSpeed = 500f;

		[SerializeField]
		protected float turningSpeedVisualScale = 0.5f;

		[SerializeField]
		protected float turningLerp = 1f;

		[SerializeField]
		protected float rapidTurnInputAngle = 140f;

		[SerializeField]
		protected float stationaryRapidTurnAngle = 90f;

		[SerializeField]
		protected float rapidTurnIgnoreInputTime = 0.05f;

		[SerializeField]
		protected int inputBufferSize = 5;

		[Header("Camera")]
		[SerializeField]
		protected float strafeLookInputScale = 20f;

		public float fallDirectionChange
		{
			get { return fallDirectionChangeSpeed; }
		}

		public AnimationCurve JumpHeightAsAFactorOfForwardSpeedAsAFactorOfSpeed
		{
			get { return jumpHeightAsAFactorOfForwardSpeed; }
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

		public float inputAngleRapidTurn
		{
			get { return rapidTurnInputAngle; }
		}

		public float stationaryAngleRapidTurn
		{
			get { return stationaryRapidTurnAngle; }
		}

		public float ignoreInputTimeRapidTurn
		{
			get { return rapidTurnIgnoreInputTime; }
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

		public int forwardInputWindowSize
		{
			get { return useCustomActionParameters ? action.forwardInputWindowSize : 1; }
		}

		public float fallingForwardSpeed
		{
			get { return fallingMaxForwardSpeed; }
		}

		public float fallSpeedDeceleration
		{
			get { return fallForwardSpeedDeceleration; }
		}

		public float fallSpeedAcceleration
		{
			get { return fallForwardSpeedAcceleration; }
		}

		public float sprintNormalizedForwardSpeedIncrease
		{
			get { return sprintNormalizedSpeedIncrease; }
		}

		public int bufferSizeInput
		{
			get { return inputBufferSize; }
		}

		public float standingJumpSpeed
		{
			get { return standingJumpForwardSpeed; }
		}

		public float standingJumpMinInputThreshold
		{
			get { return minInputThreshold; }
		}

		public float standingJumpMaxMovementThreshold
		{
			get { return maxMovementThreshold; }
		}

		public int postPhyicsJumpFramesToIgnoreForward
		{
			get { return postPhyicsJumpFramesToIgnoreForwardSpeed; }
		}
	}
}
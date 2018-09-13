using StandardAssets.Characters.Attributes;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Data model class containing various settings for the <see cref="ThirdPersonAnimationController"/>.
	/// </summary>
	[CreateAssetMenu(fileName = "Third Person Animation Configuration", menuName = "Standard Assets/Characters/Third Person Animation Configuration", order = 1)]
	public class ThirdPersonAnimationConfiguration : ScriptableObject
	{
		[SerializeField, Tooltip("Animator used to find the below state names.")]
		protected RuntimeAnimatorController thirdPersonAnimator;
		
		[Header("State Names")]
		[SerializeField, AnimatorStateName("thirdPersonAnimator")]
		protected string locomotion = "Locomotion Blend";

		[SerializeField, AnimatorStateName("thirdPersonAnimator")]
		protected string rightFootRootMotionJump = "RightFootRootMotionJump",
						 leftFootRootMotionJump = "LeftFootRootMotionJump",
						 rightFootJump = "RightFootPhysicsJump",
						 leftFootJump = "LeftFootPhysicsJump",
						 rollLand = "RollLand",
						 land = "Land";
		
		[Header("Ground Movement"), Tooltip("Configuration for the forward speed animation parameter"), 
		 AnimatorFloatParameter("thirdPersonAnimator")]
		[SerializeField]
		protected AnimationFloatParameter forwardSpeedParameter = new AnimationFloatParameter("ForwardSpeed", 0.05f, 0.15f);
		
		[SerializeField, Tooltip("Configuration for the lateral speed animation parameter"), 
		 AnimatorFloatParameter("thirdPersonAnimator")]
		protected AnimationFloatParameter lateralSpeedParameter = new AnimationFloatParameter("LateralSpeed", 0.01f, 0.05f);

		[SerializeField, Tooltip("Configuration for the turning speed animation parameter"), 
		 AnimatorFloatParameter("thirdPersonAnimator")]
		protected AnimationFloatParameter turningSpeedParameter = new AnimationFloatParameter("TurningSpeed", 0.01f, 0.05f);

		[SerializeField, Tooltip("Name of the strafe bool animator parameter"),
		 AnimatorParameterName("thirdPersonAnimator", AnimatorControllerParameterType.Bool)]
		protected string strafeParameter = "Strafe";
		
		[Header("Jumping"), AnimatorParameterName("thirdPersonAnimator", AnimatorControllerParameterType.Float)]
		[SerializeField]
		protected string verticalSpeedParameter = "VerticalSpeed";
		
		[SerializeField, AnimatorParameterName("thirdPersonAnimator", AnimatorControllerParameterType.Trigger)]
		protected string fallParameter = "Fall";

		[SerializeField, Tooltip("Curve used to determine the cross fade duration of the transition into the jump " +
								 "animation state")]
		protected AnimationCurve jumpTransitionAsAFactorOfSpeed = AnimationCurve.Constant(0, 1, 0.15f);

		[SerializeField, Tooltip("Curve used to determine the cross fade duration of the transition into the " +
								 "locomotion animation from the jump animation state")]
		protected AnimationCurve jumpEndTransitionDurationByForwardSpeed = AnimationCurve.Linear(0,0,1,0.125f);
		
		[SerializeField, Tooltip("Cross fade cycle offset for transition into locomotion state after a physics jump")]
		protected float rightFootPhysicsJumpLandAnimationTimeOffset = 0.1f,
						leftFootPhysicsJumpLandAnimationTimeOffset = 0.6f;
		
		[SerializeField, AnimatorParameterName("thirdPersonAnimator", AnimatorControllerParameterType.Float)]
		protected string jumpedLateralSpeedParameter = "JumpedLateralSpeed";
		
		[SerializeField, AnimatorParameterName("thirdPersonAnimator", AnimatorControllerParameterType.Float)]
		protected string jumpedForwardSpeedParameter = "JumpedForwardSpeed";

		[SerializeField, Tooltip("Time in seconds allowed between jumps to create a skip effect")]
		protected float skipJumpLandWindow = 0.25f;

		[SerializeField, Tooltip("A forward jump speed less than this will be clamped to 0")]
		protected float standingJumpNormalizedSpeedMaxThreshold = 0.1f;
		
		[SerializeField, Tooltip("A forward jump speed more than this will be clamped to 1")]
		protected float runningJumpNormalizedSpeedMinThreshold = 0.9f;
		
		[Header("Landing")]
		[SerializeField, Tooltip("Curve used to determine the land animation speed")]
		protected AnimationCurve landSpeedAsAFactorOfSpeed = AnimationCurve.Linear(0,1,1,2);

		[SerializeField, Tooltip("A forward speed higher than this will trigger a roll on land")]
		protected float normalizedForwardSpeedToRoll = 0.3f;

		[SerializeField, Tooltip("A fall time greater than this will trigger a roll. Less than this will transition to" +
								 "locomotion")]
		protected float fallTimeRequiredToTriggerRoll = 1.0f;

		[SerializeField, Tooltip("Time used for the cross fade into the roll animation state")]
		protected float rollAnimationBlendTime = 0.15f;

		[SerializeField, Tooltip("Time used for the cross fade into the land animation state")]
		protected float landAnimationBlendTime = 0.11f;

		[Header("Grounded Foot")]
		[SerializeField, AnimatorParameterName("thirdPersonAnimator", AnimatorControllerParameterType.Bool)]
		protected string groundedFootRightParameter = "OnRightFoot";
		
		[SerializeField, Tooltip("Should the right foot start as grounded?")]
		protected bool startRightFootGrounded;

		[SerializeField, Tooltip("Value used to determine the grounded foot based on animation normalized time")]
		protected float groundedFootThreshold = 0.25f, groundedFootThresholdOffset = 0.25f;

		[Header("Head Movement"), Tooltip("Should the head look be turned off?")]
		[SerializeField]
		protected bool enableHeadTurn = true;

		[VisibleIf("disableHeadTurn",false)]
		[SerializeField, Tooltip("Configuration for the head turning/looking")]
		protected HeadTurnProperties headTurnProperties;

		/// <summary>
		/// Gets the forward speed parameter configuration
		/// </summary>
		public AnimationFloatParameter forwardSpeed
		{
			get { return forwardSpeedParameter; }
		}

		/// <summary>
		/// Gets the lateral speed parameter configuration
		/// </summary>
		public AnimationFloatParameter lateralSpeed
		{
			get { return lateralSpeedParameter; }
		}

		/// <summary>
		/// Gets the turning speed parameter configuration
		/// </summary>
		public AnimationFloatParameter turningSpeed
		{
			get { return turningSpeedParameter; }
		}

		/// <summary>
		/// Gets the vertical speed parameter name
		/// </summary>
		public string verticalSpeedParameterName
		{
			get { return verticalSpeedParameter; }
		}

		/// <summary>
		/// Gets the fall parameter name
		/// </summary>
		public string fallParameterName
		{
			get { return fallParameter; }
		}

		/// <summary>
		/// Gets the grounded foot right parameter name
		/// </summary>
		public string groundedFootRightParameterName
		{
			get { return groundedFootRightParameter; }
		}

		/// <summary>
		/// Gets the jumped lateral speed parameter name
		/// </summary>
		public string jumpedLateralSpeedParameterName
		{
			get { return jumpedLateralSpeedParameter; }
		}

		/// <summary>
		/// Gets the strafe parameter name
		/// </summary>
		public string strafeParameterName
		{
			get { return strafeParameter; }
		}

		/// <summary>
		/// Gets the jumped forward speed parameter name
		/// </summary>
		public string jumpedForwardSpeedParameterName
		{
			get { return jumpedForwardSpeedParameter; }
		}

		/// <summary>
		/// Gets whether the right foot should start as grounded
		/// </summary>
		/// <value>True if the right foot should start grounded; false if the left foot should.</value>
		public bool invertFoot
		{
			get { return startRightFootGrounded; }
		}

		/// <summary>
		/// Gets the threshold value used in determining the current grounded foot.
		/// </summary>
		public float groundedFootThresholdValue
		{
			get { return groundedFootThreshold; }
		}

		/// <summary>
		/// Gets the threshold offset value used in determining the current grounded foot.
		/// </summary>
		public float groundedFootThresholdOffsetValue
		{
			get { return groundedFootThresholdOffset; }
		}

		/// <summary>
		/// Gets whether the head turning/look at should be enabled
		/// </summary>
		/// <value>True if the head turning should enabled; false if it is to be disabled. </value>
		public bool enableHeadLookAt
		{
			get { return enableHeadTurn; }
		}

		/// <summary>
		/// Gets the head turn look at weight.
		/// </summary>
		public float lookAtWeight
		{
			get { return headTurnProperties.lookAtWeight; }
		}

		/// <summary>
		/// Gets the head turn look at max rotation.
		/// </summary>
		public float lookAtMaxRotation
		{
			get { return headTurnProperties.lookAtMaxRotation; }
		}

		/// <summary>
		/// Gets the head turn look at rotation speed.
		/// </summary>
		public float lookAtRotationSpeed
		{
			get { return headTurnProperties.lookAtRotationSpeed; }
		}
		
		/// <summary>
		/// Gets whether head turning should be disabled in an aerial state.
		/// </summary>
		public bool lookAtWhileAerial
		{
			get { return headTurnProperties.lookAtWhileAerial; }
		}

		/// <summary>
		/// Gets whether head turning should be disabled in a turnaround state.
		/// </summary>
		public bool lookAtWhileTurnaround
		{
			get { return headTurnProperties.lookAtWhileTurnaround; }
		}

		/// <summary>
		/// Gets the curve to be used to evaluate the transition duration out of the jump state.
		/// </summary>
		public AnimationCurve jumpEndTransitionByForwardSpeed
		{
			get { return jumpEndTransitionDurationByForwardSpeed; }
		}

		/// <summary>
		/// Gets the offset used during the cross fade out of right foot physics jump.
		/// </summary>
		public float rightFootPhysicsJumpLandAnimationOffset
		{
			get { return rightFootPhysicsJumpLandAnimationTimeOffset; }
		}

		/// <summary>
		/// Gets the offset used during the cross fade out of left foot physics jump.
		/// </summary>
		public float leftFootPhysicsJumpLandAnimationOffset
		{
			get { return leftFootPhysicsJumpLandAnimationTimeOffset; }
		}
		
		/// <summary>
		/// Gets the time allowed between physics jumps to alternate the grounded foot.
		/// </summary>
		public float skipJumpWindow
		{
			get { return skipJumpLandWindow; }
		}

		/// <summary>
		/// Gets the curve to be used to evaluate the animation speed of a land animation.
		/// </summary>
		public AnimationCurve landSpeedAsAFactorSpeed
		{
			get { return landSpeedAsAFactorOfSpeed; }
		}

		/// <summary>
		/// Gets the normalized forward speed required to initiate a roll during a land.
		/// </summary>
		public float forwardSpeedToRoll
		{
			get { return normalizedForwardSpeedToRoll; }
		}

		/// <summary>
		/// Gets the fall time in seconds required to trigger a roll on land.
		/// </summary>
		public float fallTimeRequiredToRoll
		{
			get { return fallTimeRequiredToTriggerRoll; }
		}

		/// <summary>
		/// Gets the duration of the transition into the land animator state.
		/// </summary>
		public float landAnimationBlendDuration
		{
			get { return landAnimationBlendTime; }
		}

		/// <summary>
		/// Gets the duration of the transition into the roll land animator state.
		/// </summary>
		public float rollAnimationBlendDuration
		{
			get { return rollAnimationBlendTime; }
		}

		/// <summary>
		/// Gets the locomotion animator state name.
		/// </summary>
		public string locomotionStateName
		{
			get { return locomotion; }
		}

		/// <summary>
		/// Gets the left foot physics jump animator state name.
		/// </summary>
		public string leftFootJumpStateName
		{
			get { return leftFootJump; }
		}

		/// <summary>
		/// Gets the right foot physics jump animator state name.
		/// </summary>
		public string rightFootJumpStateName
		{
			get { return rightFootJump; }
		}

		/// <summary>
		/// Gets the left foot root motion jump animator state name.
		/// </summary>
		public string leftFootRootMotionJumpStateName
		{
			get { return leftFootRootMotionJump; }
		}

		/// <summary>
		/// Gets the right foot root motion jump animator state name.
		/// </summary>
		public string rightFootRootMotionJumpStateName
		{
			get { return rightFootRootMotionJump; }
		}
		
		/// <summary>
		/// Gets the roll land animator state name
		/// </summary>
		public string rollLandStateName
		{
			get { return rollLand; }
		}

		/// <summary>
		/// Gets the land animator state name
		/// </summary>
		public string landStateName
		{
			get { return land; }
		}

		/// <summary>
		/// Gets the curve to be used to evaluate the transition duration into the jump state.
		/// </summary>
		public AnimationCurve jumpTransitionDurationFactorOfSpeed
		{
			get { return jumpTransitionAsAFactorOfSpeed; }
		}

		/// <summary>
		/// Gets the normalized speed threshold used to clamp jump forward speed to 1.
		/// </summary>
		public float runningJumpNormalizedSpeedThreshold
		{
			get { return runningJumpNormalizedSpeedMinThreshold; }
		}

		/// <summary>
		/// Gets the normalized speed threshold used to clamp jump forward speed down to 0.
		/// </summary>
		public float standingJumpNormalizedSpeedThreshold
		{
			get { return standingJumpNormalizedSpeedMaxThreshold; }
		}
	}
}
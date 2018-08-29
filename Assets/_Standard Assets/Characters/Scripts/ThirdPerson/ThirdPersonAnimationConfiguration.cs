using Attributes;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[CreateAssetMenu(fileName = "Third Person Animation Configuration", menuName = "Standard Assets/Characters/Third Person Animation Configuration", order = 1)]
	public class ThirdPersonAnimationConfiguration : ScriptableObject
	{
		[SerializeField]
		protected RuntimeAnimatorController thirdPersonAnimator;
		
		[Header("State Names")]
		[SerializeField]
		protected string locomotion = "Locomotion Blend";
		
		[SerializeField]
		protected string rightFootRootMotionJump = "OnRightFoot",
						 leftFootRootMotionJump = "OnLeftFoot",
						 rightFootJump = "OnRightFootBlend",
						 leftFootJump = "OnLeftFootBlend",
						 rollLand = "RollLand",
						 land = "Land";
		
		[Space, SerializeField]
		protected string hasInputParameter = "HasInput";
		
		[Header("Ground Movement")]
		[SerializeField]
		protected AnimationFloatParameter forwardSpeedParameter = new AnimationFloatParameter("ForwardSpeed", 0.05f, 0.15f);
		
		[SerializeField]
		protected AnimationFloatParameter lateralSpeedParameter = new AnimationFloatParameter("LateralSpeed", 0.01f, 0.05f);

		[SerializeField]
		protected AnimationFloatParameter turningSpeedParameter = new AnimationFloatParameter("TurningSpeed", 0.01f, 0.05f);
		
		[SerializeField]
		protected string groundedParameter = "Grounded";
		
		[Header("Jumping")]
		[SerializeField]
		protected string verticalSpeedParameter = "VerticalSpeed";
		
		[SerializeField]
		protected string fallParameter = "Fall";

		[SerializeField]
		protected AnimationCurve jumpTransitionAsAFactorOfSpeed = AnimationCurve.Constant(0, 1, 0.15f);

		[SerializeField]
		protected float rightFootPhysicsJumpLandAnimationTimeOffset = 0.1f,
						leftFootPhysicsJumpLandAnimationTimeOffset = 0.6f;
		[SerializeField]
		protected AnimationCurve jumpEndTransitionDurationByForwardSpeed = AnimationCurve.Linear(0,0,1,0.125f);
		
		[SerializeField]
		protected string jumpedLateralSpeedParameter = "JumpedLateralSpeed";
		
		[SerializeField]
		protected string jumpedForwardSpeedParameter = "JumpedForwardSpeed";

		[SerializeField]
		protected float skipJumpLandWindow = 0.25f;
		
		[Header("Landing")]
		[SerializeField]
		protected AnimationCurve landSpeedAsAFactorOfSpeed = AnimationCurve.Linear(0,1,1,2);

		[SerializeField]
		protected float normalizedForwardSpeedToRoll = 0.3f,
						rollAnimationBlendTime = 0.15f,
						landAnimationBlendTime = 0.11f;
		
		[Header("Turning")]
		[SerializeField]
		protected string rapidTurnParameter = "RapidTurn";

		[Header("Footedness")]
		[SerializeField]
		protected string footednessParameter = "OnRightFoot";
		
		[SerializeField]
		protected bool invertFootedness;

		[SerializeField]
		protected float footednessThreshold = 0.25f, footednessThresholdOffset = 0.25f;

		[Header("Head Movement")]
		[SerializeField]
		protected bool disableHeadTurn;

		[ConditionalInclude("disableHeadTurn",false)]
		[SerializeField]
		protected HeadTurnProperties headTurnProperties;

		public AnimationFloatParameter forwardSpeed
		{
			get { return forwardSpeedParameter; }
		}

		public AnimationFloatParameter lateralSpeed
		{
			get { return lateralSpeedParameter; }
		}

		public AnimationFloatParameter turningSpeed
		{
			get { return turningSpeedParameter; }
		}

		public string verticalSpeedParameterName
		{
			get { return verticalSpeedParameter; }
		}

		public string groundedParameterName
		{
			get { return groundedParameter; }
		}

		public string fallParameterName
		{
			get { return fallParameter; }
		}

		public string hasInputParameterName
		{
			get { return hasInputParameter; }
		}

		public string footednessParameterName
		{
			get { return footednessParameter; }
		}

		public string jumpedLateralSpeedParameterName
		{
			get { return jumpedLateralSpeedParameter; }
		}

		public string jumpedForwardSpeedParameterName
		{
			get { return jumpedForwardSpeedParameter; }
		}

		public string rapidTurnParameterName
		{
			get { return rapidTurnParameter; }
		}

		public bool invertFoot
		{
			get { return invertFootedness; }
		}

		public float footednessThresholdValue
		{
			get { return footednessThreshold; }
		}

		public float footednessThresholdOffsetValue
		{
			get { return footednessThresholdOffset; }
		}

		public bool disableHeadLookAt
		{
			get { return disableHeadTurn; }
		}

		public float lookAtWeight
		{
			get { return headTurnProperties.lookAtWeight; }
		}

		public float lookAtMaxRotation
		{
			get { return headTurnProperties.lookAtMaxRotation; }
		}

		public float lookAtRotationSpeed
		{
			get { return headTurnProperties.lookAtRotationSpeed; }
		}

		public bool lookAtWhileAerial
		{
			get { return headTurnProperties.lookAtWhileAerial; }
		}

		public bool lookAtWhileTurnaround
		{
			get { return headTurnProperties.lookAtWhileTurnaround; }
		}

		public AnimationCurve jumpEndTransitionByForwardSpeed
		{
			get { return jumpEndTransitionDurationByForwardSpeed; }
		}

		public float rightFootPhysicsJumpLandAnimationOffset
		{
			get { return rightFootPhysicsJumpLandAnimationTimeOffset; }
		}

		public float leftFootPhysicsJumpLandAnimationOffset
		{
			get { return leftFootPhysicsJumpLandAnimationTimeOffset; }
		}
		
		public float skipJumpWindow
		{
			get { return skipJumpLandWindow; }
		}

		public AnimationCurve landSpeedAsAFactorSpeed
		{
			get { return landSpeedAsAFactorOfSpeed; }
		}

		public float forwardSpeedToRoll
		{
			get { return normalizedForwardSpeedToRoll; }
		}

		public float landAnimationBlendDuration
		{
			get { return landAnimationBlendTime; }
		}

		public float rollAnimationBlendDuration
		{
			get { return rollAnimationBlendTime; }
		}

		public string locomotionStateName
		{
			get { return locomotion; }
		}

		public string leftFootJumpStateName
		{
			get { return leftFootJump; }
		}

		public string rightFootJumpStateName
		{
			get { return rightFootJump; }
		}

		public string leftFootRootMotionJumpStateName
		{
			get { return leftFootRootMotionJump; }
		}

		public string rightFootRootMotionJumpStateName
		{
			get { return rightFootRootMotionJump; }
		}

		public string rollLandStateName
		{
			get { return rollLand; }
		}

		public string landStateName
		{
			get { return land; }
		}

		public RuntimeAnimatorController animator
		{
			get { return thirdPersonAnimator; }
		}

		public AnimationCurve jumpTransitionDurationFactorOfSpeed
		{
			get { return jumpTransitionAsAFactorOfSpeed; }
		}
	}
}
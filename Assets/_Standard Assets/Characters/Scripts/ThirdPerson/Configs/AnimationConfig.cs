using System;
using StandardAssets.Characters.Helpers;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.Configs
{
	/// <summary>
	/// Data model class containing various settings for animation
	/// </summary>
	[CreateAssetMenu(fileName = "Third Person Animation Configuration", 
		menuName = "Standard Assets/Characters/Third Person Animation Configuration", order = 1)]
	public class AnimationConfig : ScriptableObject
	{
		[Serializable]
		class AdvancedAnimationConfig
		{
			[Header("Ground Movement")]
			[SerializeField, Tooltip("Configuration for the forward speed animation parameter")]
			FloatRange m_ForwardSpeedInterpolationRange = new FloatRange(0.2f, 0.35f);

			[SerializeField, Tooltip("Configuration for the lateral speed animation parameter")]
			FloatRange m_LateralSpeedInterpolationRange = new FloatRange(0.2f, 0.35f);

			[SerializeField, Tooltip("Configuration for the turning speed animation parameter")]
			FloatRange m_TurningSpeedInterpolationRange = new FloatRange(0.01f, 0.05f);
			
			[SerializeField, Tooltip("Curve used to remap raw normalized turning speed")]
			AnimationCurve m_TurningSpeedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
			
			[Header("Strafe Rapid Direction Change")]
			[SerializeField, Tooltip("Should a strafe rapid direction change be detected and smoothed. This should only " +
			                         "be enabled if opposing strafe animations are reverses of each other. eg walk " +
			                         "backwards is walk forward played at a -1 speed")]
			bool m_EnableStrafeRapidDirectionChangeSmoothing = true;
			
			[SerializeField, Tooltip("Input change angle threshold used to trigger a strafe rapid direction change")]
			float m_StrafeRapidDirectionChangeAngle = 140.0f;
			
			[SerializeField, Tooltip("Curve used to change animator movement speeds during a strafe rapid direction change")]
			AnimationCurve m_StrafeRapidDirectionChangeSpeedCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
					
			[Header("Jumping")]
			[SerializeField, Tooltip("Curve used to determine the cross fade duration of the transition into the jump " +
			                         "animation state in exploration mode")]
			AnimationCurve m_JumpTransitionAsAFactorOfSpeed = AnimationCurve.Constant(0.0f, 1.0f, 0.15f);
			[SerializeField, Tooltip("Curve used to determine the cross fade duration of the transition into the jump " +
			                         "animation state in strafe mode")]
			AnimationCurve m_StrafeJumpTransitionAsAFactorOfSpeed = AnimationCurve.Constant(0.0f, 1.0f, 0.15f);

			[SerializeField, Tooltip("Time to add to the jump blend duration based on current grounded foot's position")]
			float m_JumpBlendTimeInc = 0.05f;

			[SerializeField, Tooltip("Curved used to evaluate the current foots position in order to add Jump Blend Time Inc")]
			AnimationCurve m_FootPositionJumpIncRemap = AnimationCurve.Constant(0.0f, 0.0f, 0.0f);
			
			[SerializeField, Tooltip("Curve used to determine the cross fade duration of the transition into the " +
			                         "locomotion animation from the jump animation state")]
			AnimationCurve m_JumpEndTransitionAsAFactorOfSpeed = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 0.125f);
			
			[SerializeField, Tooltip("Cross fade cycle offset for transition into locomotion state after a right foot jump")]
			float m_RightFootJumpLandAnimationTimeOffset = 0.6f;
			
			[SerializeField, Tooltip("Cross fade cycle offset for transition into locomotion state after a left foot jump")]
			float m_LeftFootJumpLandAnimationTimeOffset = 0.3f;

			[SerializeField, Tooltip("Time in seconds allowed between jumps to create a skip effect")]
			float m_SkipJumpLandWindow = 0.38f;
			
			[Header("Landing")]
			[SerializeField, Tooltip("Curve used to determine the land animation speed")]
			AnimationCurve m_LandSpeedAsAFactorOfSpeed = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 2.0f);
			
			[SerializeField, Tooltip("Time used for the cross fade into the roll animation state")]
			float m_RollAnimationBlendTime = 0.15f;

			[SerializeField, Tooltip("Time used for the cross fade into the land animation state")]
			float m_LandAnimationBlendTime = 0.11f;
			
			[Header("Grounded Foot")]
			[SerializeField, Tooltip("Should the right foot start as grounded? Default is the left foot")]
			bool m_StartRightFootGrounded;
			
			[Header("Head Turn")]
			[SerializeField, Tooltip("Configuration for the head turning/looking")]
			HeadTurnProperties m_HeadTurnProperties;


			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public bool enableStrafeRapidDirectionChangeSmoothing { get { return m_EnableStrafeRapidDirectionChangeSmoothing; } }
			
			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public HeadTurnProperties headTurnProperties { get { return m_HeadTurnProperties; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public bool startRightFootGrounded { get { return m_StartRightFootGrounded; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public float landAnimationBlendTime { get { return m_LandAnimationBlendTime; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public float rollAnimationBlendTime { get { return m_RollAnimationBlendTime; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public AnimationCurve landSpeedAsAFactorOfSpeed { get { return m_LandSpeedAsAFactorOfSpeed; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public float skipJumpLandWindow { get { return m_SkipJumpLandWindow; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public float leftFootJumpLandAnimationTimeOffset { get { return m_LeftFootJumpLandAnimationTimeOffset; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public float rightFootJumpLandAnimationTimeOffset { get { return m_RightFootJumpLandAnimationTimeOffset; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public AnimationCurve jumpEndTransitionAsAFactorOfSpeed { get { return m_JumpEndTransitionAsAFactorOfSpeed; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public AnimationCurve strafeJumpTransitionAsAFactorOfSpeed { get { return m_StrafeJumpTransitionAsAFactorOfSpeed; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public AnimationCurve jumpTransitionAsAFactorOfSpeed { get { return m_JumpTransitionAsAFactorOfSpeed; } }
			
			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public float jumpBlendTimeInc { get { return m_JumpBlendTimeInc; } }
			
			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public AnimationCurve footPositionJumpIncRemap { get { return m_FootPositionJumpIncRemap; } }
			
			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public AnimationCurve turningSpeedCurve { get { return m_TurningSpeedCurve; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public AnimationCurve strafeRapidDirectionChangeSpeedCurve { get { return m_StrafeRapidDirectionChangeSpeedCurve; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public float strafeRapidDirectionChangeAngle{ get { return m_StrafeRapidDirectionChangeAngle; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public FloatRange turningSpeedInterpolationRange { get { return m_TurningSpeedInterpolationRange; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public FloatRange lateralSpeedInterpolationRange { get { return m_LateralSpeedInterpolationRange; } }

			/// <summary>
			/// COMMENT TODO
			/// </summary>
			public FloatRange forwardSpeedInterpolationRange { get { return m_ForwardSpeedInterpolationRange; } }
		}
		
		// values used to determine the grounded foot based on animation normalized time. These should only be changed
		// if locomotion animations are irregular.
		const float k_GroundedFootThreshold = 0.25f;
		const float k_GroundedFootThresholdOffset = 0.25f;

		[Header("Landing")]
		[SerializeField, Tooltip("A forward speed higher than this will trigger a roll on land")]
		float m_NormalizedForwardSpeedRequiredToRoll = 0.3f;

		[SerializeField, Tooltip("A fall time greater than this will trigger a roll. Less than this will transition to" +
								 "locomotion")]
		float m_FallTimeRequiredToTriggerRoll = 1.0f;

		[Header("Head Movement"), Tooltip("Should the head look be turned off?")]
		[SerializeField]
		bool m_EnableHeadTurn = true;

		[SerializeField, Space]
		AdvancedAnimationConfig m_Advanced;

		/// <summary>
		/// Gets whether strafe rapid direction smoothing logic should be performed.
		/// </summary>
		public bool enableStrafeRapidDirectionChangeSmoothing { get { return m_Advanced.enableStrafeRapidDirectionChangeSmoothing; } }
		
		/// <summary>
		/// Gets the angle threshold used to trigger a strafe rapid direction change.
		/// </summary>
		public float strafeRapidChangeAngleThreshold { get { return m_Advanced.strafeRapidDirectionChangeAngle; } }
		
		/// <summary>
		/// Gets the curve used to interpolate animator movement speeds during a strafe rapid direction change.
		/// </summary>
		public AnimationCurve strafeRapidChangeSpeedCurve { get { return m_Advanced.strafeRapidDirectionChangeSpeedCurve; } }
		
		/// <summary>
		/// Gets the animation curved used for remapping turning speed.
		/// </summary>
		public AnimationCurve animationTurningSpeedCurve { get { return m_Advanced.turningSpeedCurve; } }

		/// <summary>
		/// Gets the forward speed parameter configuration
		/// </summary>
		public FloatRange forwardSpeedInterpolation { get { return m_Advanced.forwardSpeedInterpolationRange; } }

		/// <summary>
		/// Gets the lateral speed parameter configuration
		/// </summary>
		public FloatRange lateralSpeedInterpolation { get { return m_Advanced.lateralSpeedInterpolationRange; } }

		/// <summary>
		/// Gets the turning speed parameter configuration
		/// </summary>
		public FloatRange turningSpeedInterpolation { get { return m_Advanced.turningSpeedInterpolationRange; } }

		/// <summary>
		/// Gets whether the right foot should start as grounded
		/// </summary>
		/// <value>True if the right foot should start grounded; false if the left foot should.</value>
		public bool invertFoot { get { return m_Advanced.startRightFootGrounded; } }

		/// <summary>
		/// Gets the threshold value used in determining the current grounded foot.
		/// </summary>
		public float groundedFootThresholdValue { get { return k_GroundedFootThreshold; } }

		/// <summary>
		/// Gets the threshold offset value used in determining the current grounded foot.
		/// </summary>
		public float groundedFootThresholdOffsetValue { get { return k_GroundedFootThresholdOffset; } }

		/// <summary>
		/// Gets whether the head turning/look at should be enabled
		/// </summary>
		/// <value>True if the head turning should enabled; false if it is to be disabled. </value>
		public bool enableHeadLookAt { get { return m_EnableHeadTurn; } }

		/// <summary>
		/// Gets the head turn look at weight.
		/// </summary>
		public float lookAtWeight { get { return m_Advanced.headTurnProperties.lookAtWeight; } }

		/// <summary>
		/// Gets the head turn look at max rotation.
		/// </summary>
		public float lookAtMaxRotation { get { return m_Advanced.headTurnProperties.lookAtMaxRotation; } }

		/// <summary>
		/// Gets the head turn look at rotation speed.
		/// </summary>
		public float lookAtRotationSpeed { get { return m_Advanced.headTurnProperties.lookAtRotationSpeed; } }
		
		/// <summary>
		/// Gets whether head turning should be disabled in an aerial state.
		/// </summary>
		public bool lookAtWhileAerial { get { return m_Advanced.headTurnProperties.lookAtWhileAerial; } }

		/// <summary>
		/// Gets whether head turning should be disabled in a turnaround state.
		/// </summary>
		public bool lookAtWhileTurnaround { get { return m_Advanced.headTurnProperties.lookAtWhileTurnaround; } }

		/// <summary>
		/// Gets the curve to be used to evaluate the transition duration out of the jump state.
		/// </summary>
		public AnimationCurve jumpEndTransitionAsAFactorOfSpeed { get { return m_Advanced.jumpEndTransitionAsAFactorOfSpeed; } }

		/// <summary>
		/// Gets the offset used during the cross fade out of right foot physics jump.
		/// </summary>
		public float rightFootJumpLandAnimationOffset { get { return m_Advanced.rightFootJumpLandAnimationTimeOffset; } }

		/// <summary>
		/// Gets the offset used during the cross fade out of left foot physics jump.
		/// </summary>
		public float leftFootJumpLandAnimationOffset { get { return m_Advanced.leftFootJumpLandAnimationTimeOffset; } }
		
		/// <summary>
		/// Gets the time allowed between physics jumps to alternate the grounded foot.
		/// </summary>
		public float skipJumpWindow { get { return m_Advanced.skipJumpLandWindow; } }

		/// <summary>
		/// Gets the curve to be used to evaluate the animation speed of a land animation.
		/// </summary>
		public AnimationCurve landSpeedAsAFactorSpeed { get { return m_Advanced.landSpeedAsAFactorOfSpeed; } }

		/// <summary>
		/// Gets the normalized forward speed required to initiate a roll during a land.
		/// </summary>
		public float forwardSpeedRequiredToRoll { get { return m_NormalizedForwardSpeedRequiredToRoll; } }

		/// <summary>
		/// Gets the fall time in seconds required to trigger a roll on land.
		/// </summary>
		public float fallTimeRequiredToRoll { get { return m_FallTimeRequiredToTriggerRoll; } }

		/// <summary>
		/// Gets the duration of the transition into the land animator state.
		/// </summary>
		public float landAnimationBlendDuration { get { return m_Advanced.landAnimationBlendTime; } }

		/// <summary>
		/// Gets the duration of the transition into the roll land animator state.
		/// </summary>
		public float rollAnimationBlendDuration { get { return m_Advanced.rollAnimationBlendTime; } }

		/// <summary>
		/// Gets the curve to be used to evaluate the transition duration into the jump state in exploration mode.
		/// </summary>
		public AnimationCurve jumpTransitionAsAFactorOfSpeed { get { return m_Advanced.jumpTransitionAsAFactorOfSpeed; } }
		
		/// <summary>
		/// Gets the curve to be used to evaluate the transition duration into the jump state in strafe mode.
		/// </summary>
		public AnimationCurve strafeJumpTransitionAsAFactorOfSpeed { get { return m_Advanced.strafeJumpTransitionAsAFactorOfSpeed; } }
		
		/// <summary>
		/// Gets the time to add to the jump blend duration based on current grounded foot's position
		/// </summary>
		public float jumpBlendTimeInc { get { return m_Advanced.jumpBlendTimeInc; } }
		
		/// <summary>
		/// Gets the curved used to evaluate the current foots position in order to add <see cref="jumpBlendTimeInc"/>
		/// </summary>
		public AnimationCurve footPositionJumpIncRemap { get { return m_Advanced.footPositionJumpIncRemap; } }

		/// <summary>
		/// Gets the scale applied to head look at speed when there is no look input.
		/// </summary>
		public float noLookInputHeadLookAtScale { get { return m_Advanced.headTurnProperties.noLookInputHeadLookAtScale; } }
		
		
		/// <summary>
		/// A serializable class used to store configuration settings for the head turing/look at.
		/// </summary>
		[Serializable]
		protected class HeadTurnProperties
		{
			[SerializeField, Tooltip("The animator head look at weight.")]
			float m_HeadLookAtWeight = 1f;

			[SerializeField, Tooltip("The max angle the head can rotate.")]
			float m_HeadLookAtMaxRotation = 75f;

			[SerializeField, Tooltip("The speed at which head can rotate.")]
			float m_HeadLookAtRotationSpeed = 15f;
			
			[SerializeField, Tooltip("A scale applied to look at speed when there is no look input.")]
			float m_NoInputHeadLookAtScale = 0.5f;

			[SerializeField, Tooltip("Should head rotation take place while aerial?")]
			bool m_AdjustHeadLookAtWhileAerial = true;
		
			[SerializeField, Tooltip("Should head rotation take place during rapid turnarounds?")]
			bool m_AdjustHeadLookAtDuringTurnaround = true;
		
			/// <summary>
			/// Gets the look at weight used by the animator.
			/// </summary>
			public float lookAtWeight { get { return m_HeadLookAtWeight; } }

			/// <summary>
			/// Gets the max look at rotation.
			/// </summary>
			public float lookAtMaxRotation { get { return m_HeadLookAtMaxRotation; } }

			/// <summary>
			/// Gets the rotation look at speed.
			/// </summary>
			public float lookAtRotationSpeed { get { return m_HeadLookAtRotationSpeed; } }

			/// <summary>
			/// Gets whether the head look at should be applied while aerial.
			/// </summary>
			public bool lookAtWhileAerial { get { return m_AdjustHeadLookAtWhileAerial; } }

			/// <summary>
			/// Gets whether the head look at should be applied during a turnaround.
			/// </summary>
			public bool lookAtWhileTurnaround { get { return m_AdjustHeadLookAtDuringTurnaround; } }

			/// <summary>
			/// Gets the scale applied to look at speed when there is no look input.
			/// </summary>
			public float noLookInputHeadLookAtScale { get { return m_NoInputHeadLookAtScale; } }
		}
	}
}
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson.Configs
{
	/// <summary>
	/// Data model class containing various settings for the <see cref="ThirdPersonMotor"/>.
	/// </summary>
	[CreateAssetMenu(fileName = "Third Person Motor Configuration",
		menuName = "Standard Assets/Characters/Third Person Motor Configuration", order = 1)]
	public class MotorConfig : ScriptableObject
	{
		[Serializable]
		protected class AdvancedMotorConfig
		{
			[Header("Ground Motion")]
			[SerializeField, Tooltip("During sprint normalized speed will be 1 + this. Used to extend the locomotion blend tree.")]
			float m_SprintNormalizedSpeedIncrease = 0.5f;
		
			[SerializeField, Tooltip("Number of samples used for forward input smoothing.")]
			int m_ForwardInputSamples = 5;

			[SerializeField]
			int m_StrafeInputWindowSize = 5;
			
			[SerializeField, Tooltip("When using a controller should sprint auto turn off when releasing the left analogue stick?")]
			bool m_AutoToggleSprint = true;
			
			[Header("Jump")]
			[SerializeField, Tooltip("Number of move input samples used to average forward velocity to use as jump velocity")]
			int m_JumpGroundVelocitySamples = 10;

			[SerializeField, Tooltip("Turn speed is scaled by this value during an aerial state")]
			float m_JumpTurningSpeedScale = 0.25f;
			
			[Header("Standing Jump")]
			[SerializeField, Tooltip("Minimum input allowed to trigger a standing forward jump")]
			float m_MinInputThreshold = 0.05f;
		
			[SerializeField, Tooltip("A forward movement less than this would allow a standing forward jump")]
			float m_MaxMovementThreshold = 0.75f;

			[SerializeField, Tooltip("How long after a character starts moving that a standing jump can still be initiated")]
			float m_StandingJumpMoveTimeThreshold = 0.5f;
			
			[Header("Turning")]
			[SerializeField, Tooltip("Used for effecting how much of the -1 to 1 range of normalizedTurningSpeed")]
			float m_TurningSpeedVisualScale = 1.4f;

			[SerializeField, Tooltip("Speed at which the normalized turning speed can change")]
			float m_NormalizedTurningSpeedLerpSpeed = 2f;
			
			[SerializeField, Tooltip("Rate at which normalized turn speed will return to zero when there is no turn input")]
			float m_NoLookInputTurnSpeedDeceleration = 5.0f;
			
			[SerializeField, Tooltip("A forward movement less than this would allow a standing turnaround")] 
			float m_MaxSpeedForStandingTurnaround = 0.25f;

			[SerializeField, Tooltip("Time that input will be ignored after the triggering of a rapid turn")]
			float m_RapidTurnIgnoreInputTime = 0.1f;

			[SerializeField, Tooltip("Number of frames of input that will be used to determine if a rapid turn was triggered")]
			int m_InputBufferSize = 5;

			public float sprintNormalizedSpeedIncrease
			{
				get { return m_SprintNormalizedSpeedIncrease; }
			}

			public int forwardInputWindowSize
			{
				get { return m_ForwardInputSamples; }
			}

			public int strafeInputWindowSize
			{
				get { return m_StrafeInputWindowSize; }
			}
			
			public bool autoToggleSprint
			{
				get { return m_AutoToggleSprint; }
			}

			public int jumpGroundVelocitySamples
			{
				get { return m_JumpGroundVelocitySamples; }
			}

			public float jumpTurningSpeedScale
			{
				get { return m_JumpTurningSpeedScale; }
			}

			public float minInputThreshold
			{
				get { return m_MinInputThreshold; }
			}

			public float maxMovementThreshold
			{
				get { return m_MaxMovementThreshold; }
			}

			public float standingJumpMoveTimeThreshold
			{
				get { return m_StandingJumpMoveTimeThreshold; }
			}

			public float turningSpeedVisualScale
			{
				get { return m_TurningSpeedVisualScale; }
			}

			public float normalizedTurningSpeedLerpSpeed
			{
				get { return m_NormalizedTurningSpeedLerpSpeed; }
			}

			public float maxSpeedForStandingTurnaround
			{
				get { return m_MaxSpeedForStandingTurnaround; }
			}

			public float rapidTurnIgnoreInputTime
			{
				get { return m_RapidTurnIgnoreInputTime; }
			}

			public int inputBufferSize
			{
				get { return m_InputBufferSize; }
			}
			
			public float noLookInputTurnSpeedDeceleration
			{
				get { return m_NoLookInputTurnSpeedDeceleration; }
			}
			
		}
		
		[Header("Ground Motion")]
		[SerializeField, Tooltip("Root motion will be scaled by this before movement is applied")]
		float m_RootMotionMovementScale = 1f;

		[SerializeField, Tooltip("Time it takes for the character to turn and face the camera orientation when Strafe " +
		                         "Mode has been entered")]
		float m_TurnForwardOnStartStrafeDuration = 0.125f;
		
		[SerializeField, Tooltip("Scale applied to a lateral strafe jump speed")]
		float m_LateralStrafeJumpMultiplier = 1.0f;

		[Header("Jumping")]
		[SerializeField, Tooltip("Curve used to determine jump height based on normalized forward speed")]
		AnimationCurve m_JumpHeightAsAFactorOfForwardSpeed = AnimationCurve.Constant(0,1,4);
		
		[Header("Standing Jump")]
		[SerializeField, Tooltip("Fixed jump speed used when a character initiated a Standing Forward Jump")]
		float m_StandingJumpForwardSpeed = 3.5f;

		[Header("Falling")]
		[SerializeField, Tooltip("Maximum forward speed while falling")]
		float m_FallingMaxForwardSpeed = 5.0f;

		[SerializeField, Tooltip("Rate at which falling forward speed can decrease")]
		float m_FallForwardSpeedDeceleration = 0.0025f;
		
		[SerializeField, Tooltip("Rate at which falling forward speed can increase")]
		float m_FallForwardSpeedAcceleration = 0.05f;

		[SerializeField, Tooltip("Speed at which fall direction can change")] 
		float m_FallDirectionChangeSpeed = 0.025f;

		[Header("Turning")]
		[SerializeField, Tooltip("Degrees per second that the character can turn")]
		float m_TurningSpeed = 300f;

		[SerializeField, Tooltip("Minimum angle required to trigger a rapid turn during movement")]
		float m_RapidTurnInputAngle = 140f;

		[SerializeField, Tooltip("Minimum angle required to trigger a stationary rapid")]
		float m_StationaryRapidTurnAngle = 90f;

		[FormerlySerializedAs("m_AdvancedSettings")]
		[SerializeField, Space]
		AdvancedMotorConfig m_Advanced;

		/// <summary>
		/// Gets the maximum forward speed that will trigger a standing rapid turn.
		/// </summary>
		public float standingTurnaroundSpeedThreshold
		{
			get { return m_Advanced.maxSpeedForStandingTurnaround; }
		}

		/// <summary>
		/// Gets the speed at which the fall direction can change.
		/// </summary>
		public float fallDirectionChange
		{
			get { return m_FallDirectionChangeSpeed; }
		}

		/// <summary>
		/// Gets the curve used to evaluate the jump height based on <see cref="ThirdPersonMotor.normalizedForwardSpeed"/>
		/// </summary>
		public AnimationCurve jumpHeightAsFactorOfForwardSpeed
		{
			get { return m_JumpHeightAsAFactorOfForwardSpeed; }
		}

		/// <summary>
		/// Gets the degrees per second that the character can turn.
		/// </summary>
		public float turningYSpeed
		{
			get { return m_TurningSpeed; }
		}

		/// <summary>
		/// Gets the value used for effecting how much of the -1 to 1 range of
		/// <see cref="ThirdPersonMotor.normalizedTurningSpeed"/> can use.
		/// </summary>
		public float turningSpeedScaleVisual
		{
			get { return m_Advanced.turningSpeedVisualScale; }
		}

		/// <summary>
		/// Gets the degrees per second that the character can turn during a jump.
		/// </summary>
		/// <value><see cref="m_TurningSpeed"/> with <see cref="m_JumpTurningSpeedScale"/> applied.</value>
		public float jumpTurningYSpeed
		{
			get { return m_TurningSpeed * m_Advanced.jumpTurningSpeedScale; }
		}
		
		/// <summary>
		/// Gets the rate at which normalized turn speed will return to zero when there is no turn input.
		/// </summary>
		public float noLookInputTurnSpeedDeceleration
		{
			get { return m_Advanced.noLookInputTurnSpeedDeceleration; }
		}

		/// <summary>
		/// Gets the number of samples used to average forward velocity to use as jump velocity.
		/// </summary>
		public int jumpGroundVelocityWindowSize
		{
			get { return m_Advanced.jumpGroundVelocitySamples; }
		}

		/// <summary>
		/// Gets the scale to be applied on the root motion movement before moving the character.
		/// </summary>
		public float scaleRootMovement
		{
			get { return m_RootMotionMovementScale; }
		}

		/// <summary>
		/// Gets the speed at which <see cref="ThirdPersonMotor.normalizedTurningSpeed"/> speed can change.
		/// </summary>
		public float normalizedTurningSpeedLerpSpeedFactor
		{
			get { return m_Advanced.normalizedTurningSpeedLerpSpeed; }
		}

		/// <summary>
		/// Gets the minimum angle required to trigger a rapid turn during movement.
		/// </summary>
		public float inputAngleRapidTurn
		{
			get { return m_RapidTurnInputAngle; }
		}

		/// <summary>
		/// Gets the minimum angle required to trigger a stationary rapid turn.
		/// </summary>
		public float stationaryAngleRapidTurn
		{
			get { return m_StationaryRapidTurnAngle; }
		}

		/// <summary>
		/// Gets the time in seconds to ignore input after a rapid turn is triggered.
		/// </summary>
		public float ignoreInputTimeRapidTurn
		{
			get { return m_Advanced.rapidTurnIgnoreInputTime; }
		}
		
		/// <summary>
		/// Gets the duration of the initial strafe look.
		/// </summary>
		public float turnForwardOnStartStrafeDuration
		{
			get { return m_TurnForwardOnStartStrafeDuration; }
		}
		
		/// <summary>
		/// Gets the multiplier applied to a lateral strafe jump.
		/// </summary>
		public float lateralStrafeJumpMultiplier
		{
			get { return m_LateralStrafeJumpMultiplier; }
		}

		/// <summary>
		/// Gets the strafe input window size.
		/// </summary>
		public int strafeInputWindowSize
		{
			get { return m_Advanced.strafeInputWindowSize; }
		}

		/// <summary>
		/// Gets the forward input window size.
		/// </summary>
		public int forwardInputWindowSize
		{
			get { return m_Advanced.forwardInputWindowSize;}
		}

		/// <summary>
		/// Gets the maximum falling forward speed.
		/// </summary>
		public float fallingForwardSpeed
		{
			get { return m_FallingMaxForwardSpeed; }
		}

		/// <summary>
		/// Gets the forward deceleration applied during a fall.
		/// </summary>
		public float fallSpeedDeceleration
		{
			get { return m_FallForwardSpeedDeceleration; }
		}

		/// <summary>
		/// Gets the forward acceleration applied during a fall.
		/// </summary>
		public float fallSpeedAcceleration
		{
			get { return m_FallForwardSpeedAcceleration; }
		}

		/// <summary>
		/// Gets the increase that sprint will apply to <see cref="ThirdPersonMotor.normalizedForwardSpeed"/>.
		/// </summary>
		public float sprintNormalizedForwardSpeedIncrease
		{
			get { return m_Advanced.sprintNormalizedSpeedIncrease; }
		}

		/// <summary>
		/// Gets the number of frames of input will used to determine if a rapid turn was triggered.
		/// </summary>
		public int bufferSizeInput
		{
			get { return m_Advanced.inputBufferSize; }
		}

		/// <summary>
		/// Gets the speed of a standing forward jump
		/// </summary>
		public float standingJumpSpeed
		{
			get { return m_StandingJumpForwardSpeed; }
		}

		/// <summary>
		/// Gets the minimum movement input allowed to trigger a standing forward jump.
		/// </summary>
		public float standingJumpMinInputThreshold
		{
			get { return m_Advanced.minInputThreshold; }
		}

		/// <summary>
		/// Gets the maximum movement allowed to trigger a standing forward jump.
		/// </summary>
		public float standingJumpMaxMovementThreshold
		{
			get { return m_Advanced.maxMovementThreshold; }
		}

		/// <summary>
		/// Gets whether sprint should auto disable when there is no input.
		/// </summary>
		public bool autoToggleSprintOnNoInput
		{
			get { return m_Advanced.autoToggleSprint; }
		}

		/// <summary>
		/// Gets the time, in seconds, allowed after movement from idle that a standing jump can be triggered.
		/// </summary>
		public float standingJumpMoveThresholdTime
		{
			get { return m_Advanced.standingJumpMoveTimeThreshold; }
		}
	}
}
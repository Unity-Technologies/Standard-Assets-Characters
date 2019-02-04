using System;
using UnityEngine;

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
		class AdvancedMotorConfig
		{
			[Header("Ground Motion")]
			[SerializeField, Tooltip("Number of samples used for forward input smoothing.")]
			int m_ForwardInputSamples = 5;

			[SerializeField, Tooltip("Number of samples used for strafe input smoothing.")]
			int m_StrafeInputWindowSize = 5;
			
			[SerializeField, Tooltip("When using a controller should sprint auto turn off when releasing the left analogue stick?")]
			bool m_AutoToggleSprint = true;
			
			[Header("Jump")]
			[SerializeField, Tooltip("Number of move input samples used to average forward velocity to use as jump velocity")]
			int m_ForwardVelocitySamples = 10;

			[SerializeField, Tooltip("Turn speed is scaled by this value during an aerial state")]
			float m_AirTurnSpeedScale = 0.25f;
			
			[Header("Standing Jump")]
			[SerializeField, Tooltip("Minimum input allowed to trigger a standing forward jump")]
			float m_MinInputThreshold = 0.05f;
		
			[SerializeField, Tooltip("A forward movement of the character less than this would allow a standing forward jump")]
			float m_MaxMovementThreshold = 0.75f;

			[SerializeField, Tooltip("How long after a character starts moving that a standing jump can still be initiated")]
			float m_MovementTimeThreshold = 0.5f;
			
			[Header("Turning")]
			[SerializeField, Tooltip("Used for effecting how much of the -1 to 1 range of normalizedTurningSpeed")]
			float m_TurnSpeedScale = 1.4f;

			[SerializeField, Tooltip("Normalized percentage per second normalized turn speed will return to zero when there is no turn input")]
			float m_TurnSpeedDecay = 5.0f;
			
			[SerializeField, Tooltip("Speed at which the normalized turning speed can change")]
			float m_NormalizedTurnSpeedDelta = 2f;
			
			[SerializeField, Tooltip("A forward movement less than this would allow a standing turn around")] 
			float m_StandingTurnAroundSpeed = 0.25f;

			[SerializeField, Tooltip("Time (in seconds) that input will be ignored after the triggering of a rapid turn")]
			float m_TurnAroundIgnoreTime = 0.1f;

			[SerializeField, Tooltip("Number of frames of input that will be used to determine if a rapid turn was triggered")]
			int m_InputBufferSize = 5;

			/// <summary>
			/// Gets the window size for sampling forward input.
			/// </summary>
			public int forwardInputWindowSize { get { return m_ForwardInputSamples; } }

			/// <summary>
			/// Gets the window size for sampling strafe input.
			/// </summary>
			public int strafeInputWindowSize { get { return m_StrafeInputWindowSize; } }
			
			/// <summary>
			/// Gets whether sprint should auto disable when there is no input.
			/// </summary>
			public bool autoToggleSprint { get { return m_AutoToggleSprint; } }

			/// <summary>
			/// Gets the number of samples used to average forward velocity to use as jump velocity.
			/// </summary>
			public int jumpGroundVelocitySamples { get { return m_ForwardVelocitySamples; } }

	        /// <summary>
	        /// Gets the jump turning speed scale.
	        /// </summary>
			public float jumpTurningSpeedScale { get { return m_AirTurnSpeedScale; } }

			/// <summary>
			/// Gets the minimum movement input allowed to trigger a standing forward jump.
			/// </summary>
			public float minInputThreshold { get { return m_MinInputThreshold; } }

			/// <summary>
			/// Gets the maximum movement allowed to trigger a standing forward jump.
			/// </summary>
			public float maxMovementThreshold { get { return m_MaxMovementThreshold; } }

			/// <summary>
			/// Gets the time, in seconds, allowed after movement from idle that a standing jump can be triggered.
			/// </summary>
			public float standingJumpMoveTimeThreshold { get { return m_MovementTimeThreshold; } }

			/// <summary>
			/// Gets the value used for effecting how much of the -1 to 1 range of
			/// <see cref="ThirdPersonMotor.normalizedTurningSpeed"/> can use.
			/// </summary>
			public float turningSpeedVisualScale { get { return m_TurnSpeedScale; } }

			/// <summary>
			/// Gets the speed at which <see cref="ThirdPersonMotor.normalizedTurningSpeed"/> speed can change.
			/// </summary>
			public float normalizedTurningSpeedLerpSpeed { get { return m_NormalizedTurnSpeedDelta; } }

			/// <summary>
			/// Gets the maximum forward speed that will trigger a standing rapid turn.
			/// </summary>
			public float standingTurnThreshold { get { return m_StandingTurnAroundSpeed; } }

			/// <summary>
			/// Gets the time in seconds to ignore input after a rapid turn is triggered.
			/// </summary>
			public float rapidTurnIgnoreInputTime { get { return m_TurnAroundIgnoreTime; } }

			/// <summary>
			/// Gets the number of frames of input will used to determine if a rapid turn was triggered.
			/// </summary>
			public int inputBufferSize { get { return m_InputBufferSize; } }
			
			/// <summary>
			/// Gets the rate at which normalized turn speed will return to zero when there is no turn input.
			/// </summary>
			/// <value>Percentage per second.</value>
			public float noLookInputTurnSpeedDeceleration { get { return m_TurnSpeedDecay; } }
		}
		
		[SerializeField, Tooltip("The default movement config. Will be overriden if configs are setup on the " +
			 "Animator locomotion states.")]
		GroundMovementConfig m_DefaultGroundMovementConfig;

		[SerializeField, Tooltip("Should the Default Config be used for every state?")]
		bool m_AlwaysUseDefaultConfig;
		
		[SerializeField, Tooltip("Time it takes for the character to turn and face the camera orientation when Strafe " +
		                         "Mode has been entered")]
		float m_StrafeOrientTime = 0.125f;
		
		[SerializeField, Tooltip("Scale applied to a lateral strafe jump speed")]
		float m_LateralStrafeJumpScale = 1.0f;

		[Header("Jumping")]
		[SerializeField, Tooltip("Curve used to determine jump height based on normalized forward speed")]
		AnimationCurve m_JumpHeightMap = AnimationCurve.Constant(0,1,4);
		
		[Header("Standing Jump")]
		[SerializeField, Tooltip("Fixed jump speed used when a character initiated a Standing Forward Jump")]
		float m_StandingJumpSpeed = 3.5f;

		[Header("Falling")]
		[SerializeField, Tooltip("Maximum forward speed while falling")]
		float m_FallForwardSpeedMax = 5.0f;
		
		[SerializeField, Tooltip("Rate at which falling forward speed can increase")]
		float m_FallForwardSpeedInc = 0.05f;

		[SerializeField, Tooltip("Rate at which falling forward speed can decrease")]
		float m_FallForwardSpeedDecay = 0.0025f;

		[SerializeField, Tooltip("Speed at which fall direction can change")] 
		float m_FallDirectionDelta = 0.025f;

		[Header("Turning")]
		[SerializeField, Tooltip("Degrees per second that the character can turn")]
		float m_TurningSpeed = 300f;

		[SerializeField, Tooltip("Minimum angle required to trigger a turn around during movement")]
		float m_TurnAroundAngle = 140f;

		[SerializeField, Tooltip("Minimum angle required to trigger a stationary rapid")]
		float m_StandingTurnAroundAngle = 90f;

		[SerializeField, Space]
		AdvancedMotorConfig m_Advanced;

		/// <summary>
		/// Gets the default GroundMovementConfig.
		/// </summary>
		public GroundMovementConfig defaultGroundMovementConfig
		{
			get { return m_DefaultGroundMovementConfig; }
		}

		/// <summary>
		/// Gets whether <see cref="defaultGroundMovementConfig"/> should always be used.
		/// </summary>
		public bool alwaysUseDefaultConfig
		{
			get { return m_AlwaysUseDefaultConfig; }
		}

		/// <summary>
		/// Gets the maximum forward speed that will trigger a standing rapid turn.
		/// </summary>
		public float standingTurnThreshold { get { return m_Advanced.standingTurnThreshold; } }

		/// <summary>
		/// Gets the speed at which the fall direction can change.
		/// </summary>
		public float fallDirectionChange { get { return m_FallDirectionDelta; } }

		/// <summary>
		/// Gets the curve used to evaluate the jump height based on <see cref="ThirdPersonMotor.normalizedForwardSpeed"/>
		/// </summary>
		public AnimationCurve jumpHeightAsFactorOfForwardSpeed { get { return m_JumpHeightMap; } }

		/// <summary>
		/// Gets the radians per second that the character can turn.
		/// </summary>
		public float turningYSpeed { get { return Mathf.Deg2Rad * m_TurningSpeed; } }

		/// <summary>
		/// Gets the value used for effecting how much of the -1 to 1 range of
		/// <see cref="ThirdPersonMotor.normalizedTurningSpeed"/> can use.
		/// </summary>
		public float turningSpeedScaleVisual { get { return m_Advanced.turningSpeedVisualScale; } }

		/// <summary>
		/// Gets the degrees per second that the character can turn during a jump.
		/// </summary>
		/// <value><see cref="m_TurningSpeed"/> with <see cref="AdvancedMotorConfig.m_AirTurnSpeedScale"/> applied.</value>
		public float jumpTurningYSpeed { get { return m_TurningSpeed * m_Advanced.jumpTurningSpeedScale; } }
		
		/// <summary>
		/// Gets the rate at which normalized turn speed will return to zero when there is no turn input.
		/// </summary>
		public float noLookInputTurnSpeedDeceleration { get { return m_Advanced.noLookInputTurnSpeedDeceleration; } }

		/// <summary>
		/// Gets the number of samples used to average forward velocity to use as jump velocity.
		/// </summary>
		public int jumpGroundVelocityWindowSize { get { return m_Advanced.jumpGroundVelocitySamples; } }

		/// <summary>
		/// Gets the speed at which <see cref="ThirdPersonMotor.normalizedTurningSpeed"/> speed can change.
		/// </summary>
		public float normalizedTurningSpeedLerpSpeedFactor { get { return m_Advanced.normalizedTurningSpeedLerpSpeed; } }

		/// <summary>
		/// Gets the minimum angle required to trigger a rapid turn during movement.
		/// </summary>
		public float inputAngleRapidTurn { get { return m_TurnAroundAngle; } }

		/// <summary>
		/// Gets the minimum angle required to trigger a stationary rapid turn.
		/// </summary>
		public float stationaryAngleRapidTurn { get { return m_StandingTurnAroundAngle; } }

		/// <summary>
		/// Gets the time in seconds to ignore input after a rapid turn is triggered.
		/// </summary>
		public float ignoreInputTimeRapidTurn { get { return m_Advanced.rapidTurnIgnoreInputTime; } }
		
		/// <summary>
		/// Gets the duration of the initial strafe look.
		/// </summary>
		public float turnForwardOnStartStrafeDuration { get { return m_StrafeOrientTime; } }
		
		/// <summary>
		/// Gets the multiplier applied to a lateral strafe jump.
		/// </summary>
		public float lateralStrafeJumpMultiplier { get { return m_LateralStrafeJumpScale; } }

		/// <summary>
		/// Gets the window size for sampling strafe input.
		/// </summary>
		public int strafeInputWindowSize { get { return m_Advanced.strafeInputWindowSize; } }

		/// <summary>
		/// Gets the window size for sampling forward input.
		/// </summary>
		public int forwardInputWindowSize { get { return m_Advanced.forwardInputWindowSize;} }

		/// <summary>
		/// Gets the maximum falling forward speed.
		/// </summary>
		public float fallingForwardSpeed { get { return m_FallForwardSpeedMax; } }

		/// <summary>
		/// Gets the forward deceleration applied during a fall.
		/// </summary>
		public float fallSpeedDeceleration { get { return m_FallForwardSpeedDecay; } }

		/// <summary>
		/// Gets the forward acceleration applied during a fall.
		/// </summary>
		public float fallSpeedAcceleration { get { return m_FallForwardSpeedInc; } }

		/// <summary>
		/// Gets the number of frames of input will used to determine if a rapid turn was triggered.
		/// </summary>
		public int bufferSizeInput { get { return m_Advanced.inputBufferSize; } }

		/// <summary>
		/// Gets the speed of a standing forward jump
		/// </summary>
		public float standingJumpSpeed { get { return m_StandingJumpSpeed; } }

		/// <summary>
		/// Gets the minimum movement input allowed to trigger a standing forward jump.
		/// </summary>
		public float standingJumpMinInputThreshold { get { return m_Advanced.minInputThreshold; } }

		/// <summary>
		/// Gets the maximum movement allowed to trigger a standing forward jump.
		/// </summary>
		public float standingJumpMaxMovementThreshold { get { return m_Advanced.maxMovementThreshold; } }

		/// <summary>
		/// Gets whether sprint should auto disable when there is no input.
		/// </summary>
		public bool autoToggleSprint { get { return m_Advanced.autoToggleSprint; } }

		/// <summary>
		/// Gets the time, in seconds, allowed after movement from idle that a standing jump can be triggered.
		/// </summary>
		public float standingJumpMoveTimeThreshold { get { return m_Advanced.standingJumpMoveTimeThreshold; } }
	}
}
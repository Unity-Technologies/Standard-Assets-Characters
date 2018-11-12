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
		[FormerlySerializedAs("autoToggleSprint")]
		[Header("Ground Motion")]
		[SerializeField, Tooltip("When using a controller should sprint auto turn off when releasing the left analogue stick?")]
		bool m_AutoToggleSprint = true;
		
		[FormerlySerializedAs("rootMotionMovementScale")]
		[SerializeField, Tooltip("Root motion will be scaled by this before movement is applied")]
		float m_RootMotionMovementScale = 1f;

		[FormerlySerializedAs("sprintNormalizedSpeedIncrease")]
		[SerializeField, Tooltip("During sprint normalized speed will be 1 + this. Used to extend the locomotion blend tree.")]
		float m_SprintNormalizedSpeedIncrease = 0.5f;
		
		[FormerlySerializedAs("useCustomExplorationParameters")]
		[SerializeField]
		bool m_UseCustomExplorationParameters = true;

		[FormerlySerializedAs("exploration")]
		[SerializeField]
		ExplorationProperties m_Exploration;

		[FormerlySerializedAs("useCustomStrafeParameters")]
		[SerializeField]
		bool m_UseCustomStrafeParameters = true;

		[FormerlySerializedAs("strafing")]
		[SerializeField]
		StrafeProperties m_Strafing;
		
		[FormerlySerializedAs("initialStrafeLookTime")]
		[SerializeField, Tooltip("Time it takes for the character to turn and face the camera orientation when Strafe " +
		                         "Mode has been entered")]
		float m_InitialStrafeLookTime = 0.125f;
		
		[SerializeField, Tooltip("Scale applied to a lateral strafe jump speed")]
		float m_lateralStrafeJumpMultiplier = 1.0f;

		[FormerlySerializedAs("jumpHeightAsAFactorOfForwardSpeed")]
		[Header("Jumping")]
		[SerializeField, Tooltip("Curve used to determine jump height based on normalized forward speed")]
		AnimationCurve m_JumpHeightAsAFactorOfForwardSpeed = AnimationCurve.Constant(0,1,4);

		[FormerlySerializedAs("jumpGroundVelocitySamples")]
		[SerializeField, Tooltip("Number of move input samples used to average forward velocity to use as jump velocity")]
		int m_JumpGroundVelocitySamples = 10;

		[FormerlySerializedAs("jumpTurningSpeedScale")]
		[SerializeField, Tooltip("Turn speed is scaled by this value during an aerial state")]
		float m_JumpTurningSpeedScale = 0.5f;
		
		[FormerlySerializedAs("standingJumpForwardSpeed")]
		[Header("Standing Jump")]
		[SerializeField, Tooltip("Fixed jump speed used when a character initiated a Standing Forward Jump")]
		float m_StandingJumpForwardSpeed = 3.5f;

		[FormerlySerializedAs("minInputThreshold")]
		[SerializeField, Tooltip("Minimum input allowed to trigger a standing forward jump")]
		float m_MinInputThreshold = 0.5f;
		
		[FormerlySerializedAs("maxMovementThreshold")]
		[SerializeField, Tooltip("A forward movement less than this would allow a standing forward jump")]
		float m_MaxMovementThreshold = 0.01f;

		[FormerlySerializedAs("standingJumpMoveTimeThreshold")]
		[SerializeField, Tooltip("How long after a character starts moving that a standing jump can still be initiated")]
		float m_StandingJumpMoveTimeThreshold = 0.5f;

		[FormerlySerializedAs("fallingMaxForwardSpeed")]
		[Header("Falling")]
		[SerializeField, Tooltip("Maximum forward speed while falling")]
		float m_FallingMaxForwardSpeed = 5;

		[FormerlySerializedAs("fallForwardSpeedDeceleration")]
		[SerializeField, Tooltip("Rate at which falling forward speed can decrease")]
		float m_FallForwardSpeedDeceleration = 0.0025f;
		
		[FormerlySerializedAs("fallForwardSpeedAcceleration")]
		[SerializeField, Tooltip("Rate at which falling forward speed can increase")]
		float m_FallForwardSpeedAcceleration = 0.05f;

		[FormerlySerializedAs("fallDirectionChangeSpeed")]
		[SerializeField, Tooltip("Speed at which fall direction can change")] 
		float m_FallDirectionChangeSpeed = 0.025f;

		[FormerlySerializedAs("turningSpeed")]
		[Header("Turning")]
		[SerializeField, Tooltip("Degrees per second that the character can turn")]
		float m_TurningSpeed = 300f;

		[FormerlySerializedAs("turningSpeedVisualScale")]
		[SerializeField, Tooltip("Used for effecting how much of the -1 to 1 range of normalizedTurningSpeed")]
		float m_TurningSpeedVisualScale = 0.5f;

		[FormerlySerializedAs("normalizedTurningSpeedLerpSpeed")]
		[SerializeField, Tooltip("Speed at which the normalized turning speed can change")]
		float m_NormalizedTurningSpeedLerpSpeed = 2f;

		[FormerlySerializedAs("rapidTurnInputAngle")]
		[SerializeField, Tooltip("Minimum angle required to trigger a rapid turn during movement")]
		float m_RapidTurnInputAngle = 140f;

		[FormerlySerializedAs("stationaryRapidTurnAngle")]
		[SerializeField, Tooltip("Minimum angle required to trigger a stationary rapid")]
		float m_StationaryRapidTurnAngle = 90f;
		
		[FormerlySerializedAs("maxSpeedForStandingTurnaround")]
		[SerializeField, Tooltip("A forward movement less than this would allow a standing turnaround")] 
		float m_MaxSpeedForStandingTurnaround = 0.25f;

		[FormerlySerializedAs("rapidTurnIgnoreInputTime")]
		[SerializeField, Tooltip("Time that input will be ignored after the triggering of a rapid turn")]
		float m_RapidTurnIgnoreInputTime = 0.05f;

		[FormerlySerializedAs("inputBufferSize")]
		[SerializeField, Tooltip("Number of frames of input that will used to determine if a rapid turn was triggered")]
		int m_InputBufferSize = 5;

		/// <summary>
		/// Gets the maximum forward speed that will trigger a standing rapid turn.
		/// </summary>
		public float standingTurnaroundSpeedThreshold
		{
			get { return m_MaxSpeedForStandingTurnaround; }
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
			get { return m_TurningSpeedVisualScale; }
		}

		/// <summary>
		/// Gets the degrees per second that the character can turn during a jump.
		/// </summary>
		/// <value><see cref="m_TurningSpeed"/> with <see cref="m_JumpTurningSpeedScale"/> applied.</value>
		public float jumpTurningYSpeed
		{
			get { return m_TurningSpeed * m_JumpTurningSpeedScale; }
		}

		/// <summary>
		/// Gets the number of samples used to average forward velocity to use as jump velocity.
		/// </summary>
		public int jumpGroundVelocityWindowSize
		{
			get { return m_JumpGroundVelocitySamples; }
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
			get { return m_NormalizedTurningSpeedLerpSpeed; }
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
			get { return m_RapidTurnIgnoreInputTime; }
		}

		public bool CustomExplorationParametersToBeUsed
		{
			get { return m_UseCustomExplorationParameters; }
		}
		
		public bool customStrafeParametersToBeUsed
		{
			get { return m_UseCustomStrafeParameters; }
		}
		
		/// <summary>
		/// Gets the duration of the initial strafe look.
		/// </summary>
		public float initialStrafeLookDuration
		{
			get { return m_InitialStrafeLookTime; }
		}
		
		/// <summary>
		/// Gets the multiplier applied to a lateral strafe jump.
		/// </summary>
		public float lateralStrafeJumpMultiplier
		{
			get { return m_lateralStrafeJumpMultiplier; }
		}

		

		/// <summary>
		/// Gets the maximum normalized forward speed during strafe.
		/// </summary>
		/// <value>1 if <see cref="m_UseCustomStrafeParameters"/> is false otherwise returns <see cref="m_Strafing"/>'s
		/// <see cref="StrafeProperties.normalizedForwardStrafeSpeed"/></value>
		public float normalizedForwardStrafeSpeed
		{
			get { return m_UseCustomStrafeParameters ? m_Strafing.normalizedForwardStrafeSpeed : 1f; }
		}
		
		/// <summary>
		/// Gets the strafe turning speed scale.
		/// </summary>
		/// <value>1 if <see cref="m_UseCustomStrafeParameters"/> is false otherwise returns <see cref="m_Strafing"/>'s
		/// <see cref="StrafeProperties.strafeTurningSpeed"/></value>
		public float strafeTurningSpeedScale
		{
			get { return m_UseCustomStrafeParameters ? m_Strafing.strafeTurningSpeed : 1f; }
		}

		/// <summary>
		/// Gets the maximum normalized backwards speed during strafe.
		/// </summary>
		/// <value>1 if <see cref="m_UseCustomStrafeParameters"/> is false otherwise returns <see cref="m_Strafing"/>'s
		/// <see cref="StrafeProperties.normalizedBackwardStrafeSpeed"/></value>
		public float normalizedBackwardStrafeSpeed
		{
			get { return m_UseCustomStrafeParameters ? m_Strafing.normalizedBackwardStrafeSpeed : 1f; }
		}

		/// <summary>
		/// Gets the maximum normalized lateral speed during strafe.
		/// </summary>
		/// <value>1 if <see cref="m_UseCustomStrafeParameters"/> is false otherwise returns <see cref="m_Strafing"/>'s
		/// <see cref="StrafeProperties.normalizedLateralStrafeSpeed"/></value>
		public float normalizedLateralStrafeSpeed
		{
			get { return m_UseCustomStrafeParameters ? m_Strafing.normalizedLateralStrafeSpeed : 1f; }
		}

		/// <summary>
		/// Gets the strafe input window size.
		/// </summary>
		/// <value>1 if <see cref="m_UseCustomStrafeParameters"/> is false otherwise returns <see cref="m_Strafing"/>'s
		/// <see cref="StrafeProperties.strafeInputWindowSize"/></value>
		public int strafeInputWindowSize
		{
			get { return m_UseCustomStrafeParameters ? m_Strafing.strafeInputWindowSize : 1; }
		}

		/// <summary>
		/// Gets the forward input window size.
		/// </summary>
		/// <value>1 if <see cref="m_UseCustomExplorationParameters"/> is false otherwise returns <see cref="m_Strafing"/>'s
		/// <see cref="ExplorationProperties.forwardInputWindowSize"/></value>
		public int forwardInputWindowSize
		{
			get { return m_UseCustomExplorationParameters ? m_Exploration.forwardInputWindowSize : 1; }
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
			get { return m_SprintNormalizedSpeedIncrease; }
		}

		/// <summary>
		/// Gets the number of frames of input will used to determine if a rapid turn was triggered.
		/// </summary>
		public int bufferSizeInput
		{
			get { return m_InputBufferSize; }
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
			get { return m_MinInputThreshold; }
		}

		/// <summary>
		/// Gets the maximum movement allowed to trigger a standing forward jump.
		/// </summary>
		public float standingJumpMaxMovementThreshold
		{
			get { return m_MaxMovementThreshold; }
		}

		/// <summary>
		/// Gets whether sprint should auto disable when there is no input.
		/// </summary>
		public bool autoToggleSprintOnNoInput
		{
			get { return m_AutoToggleSprint; }
		}

		/// <summary>
		/// Gets the time, in seconds, allowed after movement from idle that a standing jump can be triggered.
		/// </summary>
		public float standingJumpMoveThresholdTime
		{
			get { return m_StandingJumpMoveTimeThreshold; }
		}
	}
	
	/// <summary>
	/// Class used to store the forward input window size during exploration mode.
	/// </summary>
	[Serializable]
	public class ExplorationProperties
	{
		[FormerlySerializedAs("forwardInputSamples")]
		[SerializeField, Tooltip("Number of samples used for forward input smoothing.")]
		int m_ForwardInputSamples = 1;
		
		/// <summary>
		/// Gets the forward input window size used to create a moving average.
		/// </summary>
		public int forwardInputWindowSize
		{
			get { return m_ForwardInputSamples; }
		}
	}
	
	[Serializable]
	public class StrafeProperties
	{
		[FormerlySerializedAs("strafeInputSamples")]
		[SerializeField]
		int m_StrafeInputSamples = 1;
		[FormerlySerializedAs("strafeForwardSpeed")]
		[SerializeField, Range(0f,1f)]
		float m_StrafeForwardSpeed = 1f; 
		[FormerlySerializedAs("strafeBackwardSpeed")]
		[SerializeField, Range(0f,1f)]
		float m_StrafeBackwardSpeed = 1f; 
		[FormerlySerializedAs("strafeLateralSpeed")]
		[SerializeField, Range(0f,1f)]
		float m_StrafeLateralSpeed = 1f;
		[FormerlySerializedAs("strafeTurningSpeedScale")]
		[SerializeField, Range(0f,1f)]
		float m_StrafeTurningSpeedScale = 1f;

		public float normalizedForwardStrafeSpeed
		{
			get { return m_StrafeForwardSpeed; }
		}

		public float normalizedBackwardStrafeSpeed
		{
			get { return m_StrafeBackwardSpeed; }
		}

		public float normalizedLateralStrafeSpeed
		{
			get { return m_StrafeLateralSpeed; }
		}

		public int strafeInputWindowSize
		{
			get { return m_StrafeInputSamples; }
		}
		
		public float strafeTurningSpeed
		{
			get { return m_StrafeTurningSpeedScale; }
		}
	}
}
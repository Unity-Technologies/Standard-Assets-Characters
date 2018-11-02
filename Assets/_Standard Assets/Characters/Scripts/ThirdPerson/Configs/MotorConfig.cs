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
		[Header("Ground Motion")]
		[SerializeField, Tooltip("When using a controller should sprint auto turn off when releasing the left analogue stick?")]
		protected bool autoToggleSprint = true;
		
		[SerializeField, Tooltip("The root motion will be scaled by this before movement is applied")]
		protected float rootMotionMovementScale = 1f;

		[SerializeField, Tooltip("During sprint normalized speed will be 1 + this. Used to extend the locomotion blend tree.")]
		protected float sprintNormalizedSpeedIncrease = 0.5f;
		
		[SerializeField]
		protected bool useCustomActionParameters = true;

		[SerializeField]
		protected ActionProperties action;

		[SerializeField]
		protected bool useCustomStrafeParameters = true;

		[SerializeField]
		protected StrafeProperties strafing;
		
		[SerializeField, Tooltip("The duration of the initial turn into strafe mode.")]
		protected float initialStrafeLookTime = 0.125f;

		[Header("Jumping")]
		[SerializeField, Tooltip("Curve used to determine jump height based on normalized forward speed.")]
		protected AnimationCurve jumpHeightAsAFactorOfForwardSpeed = AnimationCurve.Constant(0,1,4);

		[SerializeField, Tooltip("Number of samples used to average forward velocity to use as jump velocity.")]
		protected int jumpGroundVelocitySamples = 10;

		[SerializeField, Tooltip("Turn speed is scaled by this value during an aerial state.")]
		protected float jumpTurningSpeedScale = 0.5f;
		
		[Header("Standing Jump")]
		[SerializeField, Tooltip("Jump forward speed applied during a standing forward jump.")]
		protected float standingJumpForwardSpeed = 3.5f;

		[SerializeField, Tooltip("The minimum input allowed to trigger a standing forward jump.")]
		protected float minInputThreshold = 0.5f;
		
		[SerializeField, Tooltip("The maximum character forward movement allowed to trigger a standing forward jump.")]
		protected float maxMovementThreshold = 0.01f;

		[SerializeField, Tooltip("Time allowed after movement from idle that a standing jump can be triggered.")]
		protected float standingJumpMoveTimeThreshold = 0.5f;

		[Header("Falling")]
		[SerializeField, Tooltip("The maximum forward speed while falling.")]
		protected float fallingMaxForwardSpeed = 5;

		[SerializeField, Tooltip("The speed at which falling speed can decrease.")]
		protected float fallForwardSpeedDeceleration = 0.0025f;
		
		[SerializeField, Tooltip("The speed at which falling speed can increase.")]
		protected float fallForwardSpeedAcceleration = 0.05f;

		[SerializeField, Tooltip("The speed at which fall direction can change.")] 
		protected float fallDirectionChangeSpeed = 0.025f;

		[Header("Turning")]
		[SerializeField, Tooltip("The degrees per second that the character can turn.")]
		protected float turningSpeed = 300f;

		[SerializeField, Tooltip("Used for effecting how much of the -1 to 1 range of normalizedTurningSpeed")]
		protected float turningSpeedVisualScale = 0.5f;

		[SerializeField, Tooltip("The speed at which the normalized turning speed can change")]
		protected float normalizedTurningSpeedLerpSpeed = 2f;

		[SerializeField, Tooltip("Minimum angle required to trigger a rapid turn during movement.")]
		protected float rapidTurnInputAngle = 140f;

		[SerializeField, Tooltip("Minimum angle required to trigger a stationary rapid.")]
		protected float stationaryRapidTurnAngle = 90f;

		[SerializeField, Tooltip("Time in seconds that input will be ignore after the triggering of a rapid turn.")]
		protected float rapidTurnIgnoreInputTime = 0.05f;

		[SerializeField, Tooltip("The number of frames of input will used to determine if a rapid turn was triggered.")]
		protected int inputBufferSize = 5;

		/// <summary>
		/// Gets the speed at which the fall direction can change.
		/// </summary>
		public float fallDirectionChange
		{
			get { return fallDirectionChangeSpeed; }
		}

		/// <summary>
		/// Gets the curve used to evaluate the jump height based on <see cref="IThirdPersonMotor.normalizedForwardSpeed"/>
		/// </summary>
		public AnimationCurve jumpHeightAsFactorOfForwardSpeed
		{
			get { return jumpHeightAsAFactorOfForwardSpeed; }
		}

		/// <summary>
		/// Gets the degrees per second that the character can turn.
		/// </summary>
		public float turningYSpeed
		{
			get { return turningSpeed; }
		}

		/// <summary>
		/// Gets the value used for effecting how much of the -1 to 1 range of
		/// <see cref="IThirdPersonMotor.normalizedTurningSpeed"/> can use.
		/// </summary>
		public float turningSpeedScaleVisual
		{
			get { return turningSpeedVisualScale; }
		}

		/// <summary>
		/// Gets the degrees per second that the character can turn during a jump.
		/// </summary>
		/// <value><see cref="turningSpeed"/> with <see cref="jumpTurningSpeedScale"/> applied.</value>
		public float jumpTurningYSpeed
		{
			get { return turningSpeed * jumpTurningSpeedScale; }
		}

		/// <summary>
		/// Gets the number of samples used to average forward velocity to use as jump velocity.
		/// </summary>
		public int jumpGroundVelocityWindowSize
		{
			get { return jumpGroundVelocitySamples; }
		}

		/// <summary>
		/// Gets the scale to be applied on the root motion movement before moving the character.
		/// </summary>
		public float scaleRootMovement
		{
			get { return rootMotionMovementScale; }
		}

		/// <summary>
		/// Gets the speed at which <see cref="IThirdPersonMotor.normalizedTurningSpeed"/> speed can change.
		/// </summary>
		public float normalizedTurningSpeedLerpSpeedFactor
		{
			get { return normalizedTurningSpeedLerpSpeed; }
		}

		/// <summary>
		/// Gets the minimum angle required to trigger a rapid turn during movement.
		/// </summary>
		public float inputAngleRapidTurn
		{
			get { return rapidTurnInputAngle; }
		}

		/// <summary>
		/// Gets the minimum angle required to trigger a stationary rapid turn.
		/// </summary>
		public float stationaryAngleRapidTurn
		{
			get { return stationaryRapidTurnAngle; }
		}

		/// <summary>
		/// Gets the time in seconds to ignore input after a rapid turn is triggered.
		/// </summary>
		public float ignoreInputTimeRapidTurn
		{
			get { return rapidTurnIgnoreInputTime; }
		}

		public bool customActionParametersToBeUsed
		{
			get { return useCustomActionParameters; }
		}
		
		public bool customStrafeParametersToBeUsed
		{
			get { return useCustomStrafeParameters; }
		}
		
		/// <summary>
		/// Gets the duration of the initial strafe look.
		/// </summary>
		public float initialStrafeLookDuration
		{
			get { return initialStrafeLookTime; }
		}


		/// <summary>
		/// Gets the maximum normalized forward speed during strafe.
		/// </summary>
		/// <value>1 if <see cref="useCustomStrafeParameters"/> is false otherwise returns <see cref="strafing"/>'s
		/// <see cref="StrafeProperties.normalizedForwardStrafeSpeed"/></value>
		public float normalizedForwardStrafeSpeed
		{
			get { return useCustomStrafeParameters ? strafing.normalizedForwardStrafeSpeed : 1f; }
		}
		
		/// <summary>
		/// Gets the strafe turning speed scale.
		/// </summary>
		/// <value>1 if <see cref="useCustomStrafeParameters"/> is false otherwise returns <see cref="strafing"/>'s
		/// <see cref="StrafeProperties.strafeTurningSpeed"/></value>
		public float strafeTurningSpeedScale
		{
			get { return useCustomStrafeParameters ? strafing.strafeTurningSpeed : 1f; }
		}

		/// <summary>
		/// Gets the maximum normalized backwards speed during strafe.
		/// </summary>
		/// <value>1 if <see cref="useCustomStrafeParameters"/> is false otherwise returns <see cref="strafing"/>'s
		/// <see cref="StrafeProperties.normalizedBackwardStrafeSpeed"/></value>
		public float normalizedBackwardStrafeSpeed
		{
			get { return useCustomStrafeParameters ? strafing.normalizedBackwardStrafeSpeed : 1f; }
		}

		/// <summary>
		/// Gets the maximum normalized lateral speed during strafe.
		/// </summary>
		/// <value>1 if <see cref="useCustomStrafeParameters"/> is false otherwise returns <see cref="strafing"/>'s
		/// <see cref="StrafeProperties.normalizedLateralStrafeSpeed"/></value>
		public float normalizedLateralStrafeSpeed
		{
			get { return useCustomStrafeParameters ? strafing.normalizedLateralStrafeSpeed : 1f; }
		}

		/// <summary>
		/// Gets the strafe input window size.
		/// </summary>
		/// <value>1 if <see cref="useCustomStrafeParameters"/> is false otherwise returns <see cref="strafing"/>'s
		/// <see cref="StrafeProperties.strafeInputWindowSize"/></value>
		public int strafeInputWindowSize
		{
			get { return useCustomStrafeParameters ? strafing.strafeInputWindowSize : 1; }
		}

		/// <summary>
		/// Gets the forward input window size.
		/// </summary>
		/// <value>1 if <see cref="useCustomActionParameters"/> is false otherwise returns <see cref="strafing"/>'s
		/// <see cref="ActionProperties.forwardInputWindowSize"/></value>
		public int forwardInputWindowSize
		{
			get { return useCustomActionParameters ? action.forwardInputWindowSize : 1; }
		}

		/// <summary>
		/// Gets the maximum falling forward speed.
		/// </summary>
		public float fallingForwardSpeed
		{
			get { return fallingMaxForwardSpeed; }
		}

		/// <summary>
		/// Gets the forward deceleration applied during a fall.
		/// </summary>
		public float fallSpeedDeceleration
		{
			get { return fallForwardSpeedDeceleration; }
		}

		/// <summary>
		/// Gets the forward acceleration applied during a fall.
		/// </summary>
		public float fallSpeedAcceleration
		{
			get { return fallForwardSpeedAcceleration; }
		}

		/// <summary>
		/// Gets the increase that sprint will apply to <see cref="IThirdPersonMotor.normalizedForwardSpeed"/>.
		/// </summary>
		public float sprintNormalizedForwardSpeedIncrease
		{
			get { return sprintNormalizedSpeedIncrease; }
		}

		/// <summary>
		/// Gets the number of frames of input will used to determine if a rapid turn was triggered.
		/// </summary>
		public int bufferSizeInput
		{
			get { return inputBufferSize; }
		}

		/// <summary>
		/// Gets the speed of a standing forward jump
		/// </summary>
		public float standingJumpSpeed
		{
			get { return standingJumpForwardSpeed; }
		}

		/// <summary>
		/// Gets the minimum movement input allowed to trigger a standing forward jump.
		/// </summary>
		public float standingJumpMinInputThreshold
		{
			get { return minInputThreshold; }
		}

		/// <summary>
		/// Gets the maximum movement allowed to trigger a standing forward jump.
		/// </summary>
		public float standingJumpMaxMovementThreshold
		{
			get { return maxMovementThreshold; }
		}

		/// <summary>
		/// Gets whether sprint should auto disable when there is no input.
		/// </summary>
		public bool autoToggleSprintOnNoInput
		{
			get { return autoToggleSprint; }
		}

		/// <summary>
		/// Gets the time, in seconds, allowed after movement from idle that a standing jump can be triggered.
		/// </summary>
		public float standingJumpMoveThresholdTime
		{
			get { return standingJumpMoveTimeThreshold; }
		}
	}
	
	/// <summary>
	/// Class used to store the forward input window size during action mode.
	/// </summary>
	[Serializable]
	public class ActionProperties
	{
		[SerializeField, Tooltip("Number of samples used for forward input smoothing.")]
		protected int forwardInputSamples = 1;
		
		/// <summary>
		/// Gets the forward input window size used to create a moving average.
		/// </summary>
		public int forwardInputWindowSize
		{
			get { return forwardInputSamples; }
		}
	}
	
	[Serializable]
	public class StrafeProperties
	{
		[SerializeField]
		protected int strafeInputSamples = 1;
		[SerializeField, Range(0f,1f)]
		protected float strafeForwardSpeed = 1f; 
		[SerializeField, Range(0f,1f)]
		protected float strafeBackwardSpeed = 1f; 
		[SerializeField, Range(0f,1f)]
		protected float strafeLateralSpeed = 1f;
		[SerializeField, Range(0f,1f)]
		protected float strafeTurningSpeedScale = 1f;

		public float normalizedForwardStrafeSpeed
		{
			get { return strafeForwardSpeed; }
		}

		public float normalizedBackwardStrafeSpeed
		{
			get { return strafeBackwardSpeed; }
		}

		public float normalizedLateralStrafeSpeed
		{
			get { return strafeLateralSpeed; }
		}

		public int strafeInputWindowSize
		{
			get { return strafeInputSamples; }
		}
		
		public float strafeTurningSpeed
		{
			get { return strafeTurningSpeedScale; }
		}
	}
}
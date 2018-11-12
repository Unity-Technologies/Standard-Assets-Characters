using System;
using StandardAssets.Characters.Common;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Implementation of the Third Person input
	/// </summary>
	public class ThirdPersonInput : CharacterInput, IThirdPersonInput
	{
		[FormerlySerializedAs("useInputSmoother")]
		[SerializeField, Tooltip("Should input be modified by the smoother?")]
		bool m_UseInputSmoother;
		
		/// <summary>
		/// Smooths the input movement when rotating in fast circles. This makes the character run in a circle, instead of turning around on the spot.
		/// </summary>
		[FormerlySerializedAs("locomotionInputSmoother")]
		[SerializeField, Tooltip("Smooths the input movement when rotating in fast circles. This makes the character run in a circle, instead of turning around on the spot.")]
		CharacterInputModifier m_LocomotionInputSmoother;

		/// <summary>
		/// Fired when strafe input is started
		/// </summary>
		public event Action strafeStarted;

		/// <summary>
		/// Fired when the strafe input is ended
		/// </summary>
		public event Action strafeEnded;
		
		/// <summary>
		/// Fired when the recentre camera input is applied
		/// </summary>
		public event Action recentreCamera;

		/// <summary>
		/// Tracks if the character is strafing 
		/// </summary>
		bool m_IsStrafing;
		
		/// <summary>
		/// Conditions the <paramref name="rawMoveInput"/>
		/// </summary>
		/// <param name="rawMoveInput">The move input vector received from the input action</param>
		/// <returns>The input vector conditioned by <see cref="CharacterInputModifier"/></returns>
		protected override Vector2 ConditionMoveInput(Vector2 rawMoveInput)
		{
			if (m_UseInputSmoother && m_LocomotionInputSmoother != null)
			{
				m_LocomotionInputSmoother.OnGotRawInput(ref rawMoveInput);
			}
			return rawMoveInput;
		}

		/// <summary>
		/// Registers strafe and recentre inputs.
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
			standardControls.Movement.strafe.performed += OnStrafeInput;
			standardControls.Movement.recentre.performed += OnRecentreInput;
		}

		protected override void RegisterAdditionalTouchInputs()
		{
			touchControls.Movement.strafe.performed += OnStrafeInput;
			touchControls.Movement.recentre.performed += OnRecentreInput;
		}

		/// <summary>
		/// Handles the recentre input 
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		void OnRecentreInput(InputAction.CallbackContext context)
		{
			if (recentreCamera != null)
			{
				recentreCamera();
			}
		}

		/// <summary>
		/// Handles the strafe input
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		void OnStrafeInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref m_IsStrafing, strafeStarted, strafeEnded);
		}

		/// <summary>
		/// Initializes the movement input modifier.
		/// </summary>
		void Start()
		{
			m_LocomotionInputSmoother.Init();
		}

		/// <summary>
		/// Call the modifier's OnDisable.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			if (m_LocomotionInputSmoother != null)
			{
				m_LocomotionInputSmoother.OnDisable();
			}
		}

		/// <summary>
		/// Use the modifier to update the move input.
		/// </summary>
		protected override void Update()
		{
			base.Update();
			if (m_UseInputSmoother && m_LocomotionInputSmoother != null)
			{
				moveInput = m_LocomotionInputSmoother.UpdateMoveInput();
			}
		}

		/// <summary>
		/// Sets the sprinting state to false
		/// </summary>
		public void ResetSprint()
		{
			isSprinting = false;
		}

		/// <summary>
		/// Smooths the input movement when rotating in fast circles. This makes the character run in a circle, instead of turning around on the spot.
		/// </summary>
		[Serializable]
		protected class CharacterInputModifier
		{
			/// <summary>
			/// Assume frame rate is low if it drops below this rate.
			/// </summary>
			const float k_LowFrameRateFramesPerSecond = 35.0f;

			/// <summary>
			/// Low frame rate delta time (seconds).
			/// </summary>
			const float k_LowFrameRateDeltaTime = 1.0f / k_LowFrameRateFramesPerSecond;

			/// <summary>
			/// Reset the average samples after this time (seconds).
			/// </summary>
			const float k_AverageSamplesMinTime = 0.5f;

			/// <summary>
			/// Reset the average samples after this amount of samples.
			/// </summary>
			const int k_AverageMinSamples = 5;

			/// <summary>
			/// Stop catching up to the input vector when the angle to the vector is less than this (degrees). Must be
			/// greater than zero.
			/// </summary>
			const float k_StopCatchAfterRotateAngle = 0.1f;
			
			/// <summary>
			/// The minimum move input vector magnitude require to enter the <see cref="ModifierState.RotateInCircle"/> state.
			/// </summary>
			const float k_MinInputMagnitudeForRotateInCircle = 0.8f;

			/// <summary>
			/// The modifier's state
			/// </summary>
			public enum ModifierState
			{
				/// <summary>
				/// Idle, no input.
				/// </summary>
				Idle,

				/// <summary>
				/// Move forward.
				/// </summary>
				MoveForward,

				/// <summary>
				/// Rotating in a circle.
				/// </summary>
				RotateInCircle,
			}

			/// <summary>
			/// Info about continuous input.
			/// </summary>
			public class ContinuousInfo
			{
				/// <summary>
				/// Previous input vector. This is not set to zero when there is no input.
				/// </summary>
				public Vector2 previousInput;

				/// <summary>
				/// Signed angle between current and previous input.
				/// </summary>
				public float inputAngle;

				/// <summary>
				/// The direction in which the input vector is rotated, continuously. Zero means it is not rotating.
				/// </summary>
				public float inputDirection;

				/// <summary>
				/// Count how long (seconds) input is rotated in the same direction.
				/// </summary>
				public float inputDirectionTime;

				/// <summary>
				/// Average signed angle between current and previous input.
				/// </summary>
				public float averageInputAngle;

				/// <summary>
				/// Accumulative total for the average angle.
				/// </summary>
				public float averageInputAngleTotal;

				/// <summary>
				/// Average direction in which the input vector is rotated, continuously. Zero means it is not rotating.
				/// </summary>
				public float averageInputDirection;

				/// <summary>
				/// Previous average direction, for detecting changes.
				/// </summary>
				public float previousAverageInputDirection;

				/// <summary>
				/// Count how long (seconds) the average input direction has been the same. 
				/// </summary>
				public float averageInputDirectionTime;

				/// <summary>
				/// Vector for calculating how far the input has been rotated.
				/// </summary>
				public Vector2 averageInputRotatingVector;

				/// <summary>
				/// How far the input has been rotated (degrees).
				/// </summary>
				public float averageInputRotatingTotalAngle;

				/// <summary>
				/// Count the average samples, for direction and angle.
				/// </summary>
				public int averageSamples;

				/// <summary>
				/// How long (seconds) have samples been calculated? For direction and angle.
				/// </summary>
				public float averageTime;

				/// <summary>
				/// Count how long (seconds) there was input, continuously.
				/// </summary>
				public float hasInputTime;

				/// <summary>
				/// Count how long (seconds) there was no input, continuously.
				/// </summary>
				public float noInputTime;

				/// <summary>
				/// Cound how long (seconds) the change in angle is small enough to move forward.
				/// </summary>
				public float smallAngleTime;

				/// <summary>
				/// Called when the Idle state starts. Clears the info needed to detect the next state.
				/// </summary>
				public void Clear()
				{
					inputDirection = 0.0f;
					inputDirectionTime = 0.0f;
					smallAngleTime = 0.0f;

					averageInputAngle = 0.0f;
					averageInputAngleTotal = 0.0f;
					averageInputDirection = 0.0f;
					previousAverageInputDirection = 0.0f;
					averageInputDirectionTime = 0.0f;
					averageInputRotatingVector = Vector2.zero;
					averageInputRotatingTotalAngle = 0.0f;
					averageInputAngle = 0.0f;
					averageSamples = 0;
					averageTime = 0.0f;
				}
			}

			/// <summary>
			/// The third person character brain.
			/// </summary>
			[FormerlySerializedAs("characterBrain")]
			[Header("Character")]
			[SerializeField, Tooltip("Third person character brain")]
			ThirdPersonBrain m_CharacterBrain;

			/// <summary>
			/// Delay before changing to Idle (seconds).
			/// </summary>
			[FormerlySerializedAs("changeIdleDelay")]
			[Header("Changing States")]
			[SerializeField, Tooltip("Delay before changing to Idle (seconds)")]
			float m_ChangeIdleDelay = 0.1f;

			/// <summary>
			/// Delay before changing from Idle to MoveForward (seconds).
			/// </summary>
			[FormerlySerializedAs("changeIdleToMoveDelay")]
			[SerializeField, Tooltip("Delay before changing from Idle to MoveForward (seconds)")]
			float m_ChangeIdleToMoveDelay = 0.1f;

			/// <summary>
			/// Delay before changing from RotateInCircle to MoveForward (seconds).
			/// </summary>
			[FormerlySerializedAs("changeRotateToMoveDelay")]
			[SerializeField, Tooltip("Delay before changing from RotateInCircle to MoveForward (seconds)")]
			float m_ChangeRotateToMoveDelay = 0.1f;

			/// <summary>
			/// Start MoveForward when the change in direction's angle is less than this (degrees).
			/// </summary>
			[FormerlySerializedAs("startMoveMaxAngle")]
			[SerializeField,
			 Tooltip("Start MoveForward when the change in direction's angle is less than this (degrees)")]
			float m_StartMoveMaxAngle = 1.0f;

			/// <summary>
			/// Start RotateInCircle after rotating input for this amount of time (seconds).
			/// </summary>
			[FormerlySerializedAs("startRotateInCircleMinTime")]
			[SerializeField, Tooltip("Start RotateInCircle after rotating input for this amount of time (seconds)")]
			float m_StartRotateInCircleMinTime = 1.5f;

			/// <summary>
			/// Start RotateInCircle after rotating input for this amount of degrees.
			/// </summary>
			[FormerlySerializedAs("startRotateInCircleMinTotalAngle")]
			[SerializeField, Tooltip("Start RotateInCircle after rotating input for this amount of degrees")]
			float m_StartRotateInCircleMinTotalAngle = 270.0f;

			/// <summary>
			/// Start RotateInCircle if the change in direction's angle is more than this (degrees). Must be bigger
			/// than startMoveMaxAngle.
			/// </summary>
			[FormerlySerializedAs("startRotateInCircleMinAngle")]
			[SerializeField, Tooltip(
				 "Start RotateInCircle if the change in direction's angle is more than this (degrees). " +
				 "Must be bigger than Start Move Max Angle")]
			float m_StartRotateInCircleMinAngle = 2.0f;

			/// <summary>
			/// Max angle at which to rotate the character's forward vector towards the target vector (degrees). Do not make
			/// this too small, because calculating the player's forward input vector is not 100% accurate.
			/// </summary>
			[FormerlySerializedAs("maxRotateAngle")]
			[Header("Angles")]
			[SerializeField, Tooltip(
				 "Max angle at which to rotate the character's forward vector towards the target vector " +
				 "(degrees). Do not make this too small, because calculating the player's forward input " +
				 "vector is not 100% accurate")]
			float m_MaxRotateAngle = 20.0f;

			/// <summary>
			/// When RotateInCircle ends, if angle between character's forward and current input is greater than this,
			/// then rotate to the input vector.
			/// </summary>
			[FormerlySerializedAs("catchUpToInputMinAngle")]
			[SerializeField, Tooltip("When RotateInCircle ends, if angle between character's forward and current " +
			                         "input is greater than this, then rotate to the input vector")]
			float m_CatchUpToInputMinAngle = 90.0f;

			/// <summary>
			/// Disable rapid turn before starting RotateInCircle after rotating input for this amount of time (seconds).
			/// Must be less than startRotateInCircleMinTime.
			/// </summary>
			[FormerlySerializedAs("disableRapidTurnBeforeRotateMinTime")]
			[Header("Disable Rapid Turn")]
			[SerializeField, Tooltip(
				 "Disable rapid turn before starting RotateInCircle after rotating input for this " +
				 "amount of time (seconds). Must be less than startRotateInCircleMinTime")]
			float m_DisableRapidTurnBeforeRotateMinTime = 1.0f;

			/// <summary>
			/// Disable rapid turn before starting RotateInCircle after rotating input for this amount of degrees. Must
			/// be less than startRotateInCircleMinTotalAngle.
			/// </summary>
			[FormerlySerializedAs("disableRapidTurnBeforeRotateMinAngle")]
			[SerializeField, Tooltip(
				 "Disable rapid turn before starting RotateInCircle after rotating input for this amount " +
				 "of degrees. Must be less than startRotateInCircleMinTotalAngle")]
			float m_DisableRapidTurnBeforeRotateMinAngle = 180.0f;

			/// <summary>
			/// Disable rapid turn before starting RotateInCircle after rotating input for this amount of degrees (when
			/// frame rate is low). Must be less than startRotateInCircleMinTotalAngle.
			/// </summary>
			[FormerlySerializedAs("disableRapidTurnBeforeRotateMinAngleLowFrameRate")]
			[SerializeField, Tooltip(
				 "Disable rapid turn before starting RotateInCircle after rotating input for this amount " +
				 "of degrees (when frame rate is low). Must be less than startRotateInCircleMinTotalAngle")]
			float m_DisableRapidTurnBeforeRotateMinAngleLowFrameRate = 90.0f;

			/// <summary>
			/// Enable debug in the editor?
			/// </summary>
			[FormerlySerializedAs("debugEnabled")]
			[Header("Debug")]
			[SerializeField, Tooltip("Enable debug in the editor?")]
			bool m_DebugEnabled;

			[FormerlySerializedAs("debugOffsetY")]
			[SerializeField, Tooltip("Draw vectors this Y offset from the character")]
			float m_DebugOffsetY;

			/// <summary>
			/// Character's transform
			/// </summary>
			Transform m_CharacterTransform;

			/// <summary>
			/// The root motion motor.
			/// </summary>
			ThirdPersonMotor m_Motor;

			/// <summary>
			/// The character controller controllerAdapter.
			/// </summary>
			ControllerAdapter m_ControllerAdapter;

			/// <summary>
			/// The camera to use for transforming input. It can be changed via SetCamera.
			/// </summary>
			Camera m_UnityCamera;

			/// <summary>
			/// Transform of the camera.
			/// </summary>
			Transform m_UnityCameraTransform;

			/// <summary>
			/// Was disabled in the previous update?
			/// </summary>
			bool m_WasDisabled;

			/// <summary>
			/// Does the current update have input?
			/// </summary>
			bool m_HasInput;

			/// <summary>
			/// The last received raw input.
			/// </summary>
			Vector2 m_LastRawInput;

			/// <summary>
			/// Last real time since startup.
			/// </summary>
			float m_LastRealTimeSinceStartup;

			/// <summary>
			/// The last non-zero-length move input vector.
			/// </summary>
			Vector2 m_ValidMoveInput;

			/// <summary>
			/// The character's forward vector converted to input.
			/// </summary>
			Vector2 m_CharacterForwardInput;

			/// <summary>
			/// Rotate the character's forward vector to this target input vector.
			/// </summary>
			Vector2? m_TargetMoveInput;

			/// <summary>
			/// Direction to rotate when rotating the character's forward vector to the target input vector.
			/// </summary>
			float m_TargetDirection;

			/// <summary>
			/// Catching up to the input vector after rotating in a circle?
			/// </summary>
			bool m_CatchUpToInputAfterRotating;

			/// <summary>
			/// Is the frame rate low? We handle some special cases for low frame rates.
			/// </summary>
			bool m_IsLowFrameRate;

			/// <summary>
			/// Continuous input info.
			/// </summary>
			readonly ContinuousInfo m_ContinuousInfo = new ContinuousInfo();

			/// <summary>
			/// Previous value of rotateInCircleDirection.
			/// </summary>
			float m_PreviousRotateInCircleDirection;

#if UNITY_EDITOR
			readonly bool m_DebugShowValidInput = true;
			readonly Color m_DebugValidInputColor = Color.green;
			readonly bool m_DebugShowModifiedInput = true;
			readonly Color m_DebugShowModifiedInputColor = Color.red;
			readonly bool m_DebugShowCharacterForward = true;
			readonly Color m_DebugCharacterForwardColor = Color.blue;
			readonly bool m_DebugShowCharacterForwardInput = true;
			readonly Color m_DebugShowCharacterForwardInputColor = Color.white;
#endif

			/// <summary>
			/// The modifier's state.
			/// </summary>
			public ModifierState state { get; private set; }

			/// <summary>
			/// The direction when rotating in a circle.
			/// </summary>
			public float rotateInCircleDirection { get; private set; }

			/// <summary>
			/// Called when we received raw input.
			/// </summary>
			public void OnGotRawInput(ref Vector2 rawInput)
			{
				m_LastRawInput = rawInput;
				ModifyMoveInput(ref rawInput);
			}

			/// <summary>
			/// Update the move input, based on the last raw input that was received. This simulates a polling system.
			/// </summary>
			public Vector2 UpdateMoveInput()
			{
				var moveInput = m_LastRawInput;
				ModifyMoveInput(ref moveInput);
				return moveInput;
			}
			
			/// <summary>
			/// Modify the move input.
			/// </summary>
			public void ModifyMoveInput(ref Vector2 moveInput)
			{
				m_Motor.EnableRapidTurn(this);

				// Only modify when enabled, character is grounded, and not straffing
				if (m_CharacterTransform == null || 
				    !m_CharacterTransform.gameObject.activeInHierarchy || 
				    !m_ControllerAdapter.isGrounded || 
				    m_Motor.movementMode == ThirdPersonMotorMovementMode.Strafe)
				{
					m_WasDisabled = true;
					return;
				}

				var dt = Time.realtimeSinceStartup - m_LastRealTimeSinceStartup;
				m_LastRealTimeSinceStartup = Time.realtimeSinceStartup;
				m_HasInput = moveInput.sqrMagnitude > 0.0f;
				m_IsLowFrameRate = dt > k_LowFrameRateDeltaTime;

				if (m_WasDisabled)
				{
					m_WasDisabled = false;
					SetState(ModifierState.Idle);
				}

				if (m_HasInput)
				{
					m_ValidMoveInput = moveInput;
				}

				m_TargetMoveInput = null;
				m_TargetDirection = 0.0f;
				m_CharacterForwardInput = CalculateCharacterForwardInput();

				UpdateContinuousInfo(moveInput, dt);
				CheckIfStateChanges();
				UpdateState(moveInput);
				UpdateRotation(ref moveInput);

#if UNITY_EDITOR
				if (m_DebugEnabled)
				{
					DebugUpdate(moveInput);
				}			
#endif
			}

			/// <summary>
			/// Change the camera to use for transforming input. Usually this is the main camera.
			/// </summary>
			public void SetCamera(Camera newCamera)
			{
				m_UnityCamera = newCamera;
				if (m_UnityCamera != null)
				{
					m_UnityCameraTransform = m_UnityCamera.transform;
				}
			}

			public void Init()
			{
				FindBrain();
				m_CharacterTransform = m_CharacterBrain.transform;
				m_Motor = m_CharacterBrain.thirdPersonMotor;
				m_ControllerAdapter = m_CharacterBrain.controllerAdapter;
				
				if (m_UnityCamera == null)
				{
					// Use the main camera as the default. The camera can be changed later via SetCamera.
					SetCamera(Camera.main);
					if (m_UnityCamera == null)
					{
						SetCamera(Object.FindObjectOfType<Camera>());
					}
				}

				m_LastRealTimeSinceStartup = Time.realtimeSinceStartup;
				SetState(ModifierState.Idle);
			}

			public void OnDisable()
			{
				m_WasDisabled = true;

				if (m_Motor != null)
				{
					m_Motor.EnableRapidTurn(this);
				}
			}

			/// <summary>
			/// Calculate the character's forward vector as an input vector.
			/// </summary>
			/// <returns></returns>
			Vector2 CalculateCharacterForwardInput()
			{
				var direction = m_CharacterTransform.forward;

				// Ignore height
				direction.y = 0.0f;
				direction.Normalize();

				// To local, relative to camera
				direction = m_UnityCameraTransform.InverseTransformDirection(direction);

				return new Vector2(direction.x, direction.z).normalized;
			}

			/// <summary>
			/// Helper for checking if the <see cref="ThirdPersonBrain"/> has been assigned - otherwise looks for it in the scene
			/// </summary>
			void FindBrain()
			{
				if (m_CharacterBrain == null)
				{
					var brainsInScene = Object.FindObjectsOfType<ThirdPersonBrain>();
					if (brainsInScene.Length == 0)
					{
						Debug.LogError("No ThirdPersonBrain objects in scene!");
						return;
					}

					if (brainsInScene.Length > 1)
					{
						Debug.LogWarning("Too many ThirdPersonBrain objects in scene");
						return;
					}

					m_CharacterBrain = brainsInScene[0];
				}
			}

			/// <summary>
			/// Set the modifier's state.
			/// </summary>
			void SetState(ModifierState newState)
			{
				var oldState = state;
				state = newState;

				if (oldState == ModifierState.RotateInCircle)
				{
					m_PreviousRotateInCircleDirection = rotateInCircleDirection;
				}

				switch (state)
				{
					case ModifierState.Idle:
					{
						m_ContinuousInfo.Clear();
						m_CatchUpToInputAfterRotating = false;

						break;
					}
					case ModifierState.MoveForward:
					{
						m_ContinuousInfo.averageInputRotatingVector = Vector2.zero;
						m_ContinuousInfo.averageInputRotatingTotalAngle = 0.0f;

						if (oldState == ModifierState.RotateInCircle &&
						    CalculateAngleBetweenInputAndCharacterForward(m_ValidMoveInput) > m_CatchUpToInputMinAngle)
						{
							m_CatchUpToInputAfterRotating = true;
						}

						break;
					}
					case ModifierState.RotateInCircle:
					{
						m_ContinuousInfo.smallAngleTime = 0.0f;
						rotateInCircleDirection = m_ContinuousInfo.averageInputDirection;
						m_CatchUpToInputAfterRotating = false;

						break;
					}
				}
			}

			/// <summary>
			/// Update the continous input info, which is used to help determine the modifier's state.
			/// </summary>
			void UpdateContinuousInfo(Vector2 moveInput, float dt)
			{
				if (m_HasInput)
				{
					if (m_ContinuousInfo.hasInputTime > 0.0f)
					{
						// Angle between current and previous input
						var angle = Vector2.SignedAngle(m_ContinuousInfo.previousInput, moveInput);
						var direction = !Mathf.Approximately(angle, 0.0f)
							                  ? Mathf.Sign(angle)
							                  : 0.0f;
						// Rotating in the same direction?
						if (Mathf.Approximately(m_ContinuousInfo.inputDirection, direction))
						{
							// Note: This also takes into account a direction of zero, which means input is not rotating.
							m_ContinuousInfo.inputDirectionTime += dt;
						}
						else
						{
							m_ContinuousInfo.inputDirectionTime = dt;
						}

						m_ContinuousInfo.inputAngle = angle;
						m_ContinuousInfo.inputDirection = direction;

						// Calculate averages
						m_ContinuousInfo.averageInputAngleTotal += angle;
						m_ContinuousInfo.averageSamples++;
						m_ContinuousInfo.averageTime += dt;
						m_ContinuousInfo.averageInputAngle = m_ContinuousInfo.averageInputAngleTotal /
						                                   m_ContinuousInfo.averageSamples;
						m_ContinuousInfo.averageInputDirection =
							!Mathf.Approximately(m_ContinuousInfo.averageInputAngle, 0.0f)
								? Mathf.Sign(m_ContinuousInfo.averageInputAngle)
								: 0.0f;

						if (m_ContinuousInfo.averageTime > k_AverageSamplesMinTime &&
						    m_ContinuousInfo.averageSamples > k_AverageMinSamples)
						{
							m_ContinuousInfo.averageInputAngleTotal = 0.0f;
							m_ContinuousInfo.averageSamples = 0;
							m_ContinuousInfo.averageTime = 0.0f;
						}

						// Average rotating in the same direction?
						if (Mathf.Approximately(m_ContinuousInfo.averageInputDirection,
						                        m_ContinuousInfo.previousAverageInputDirection))
						{
							// Note: This also takes into account a direction of zero, which means input is not rotating.
							m_ContinuousInfo.averageInputDirectionTime += dt;
							m_ContinuousInfo.averageInputRotatingTotalAngle +=
								Vector2.Angle(moveInput, m_ContinuousInfo.averageInputRotatingVector);
						}
						else
						{
							m_ContinuousInfo.averageInputDirectionTime = dt;
							m_ContinuousInfo.averageInputRotatingTotalAngle = 0.0f;
						}

						m_ContinuousInfo.previousAverageInputDirection = m_ContinuousInfo.averageInputDirection;
						m_ContinuousInfo.averageInputRotatingVector = moveInput;

						if (Mathf.Abs(m_ContinuousInfo.averageInputAngle) < m_StartMoveMaxAngle)
						{
							m_ContinuousInfo.smallAngleTime += dt;
						}
						else
						{
							m_ContinuousInfo.smallAngleTime = 0.0f;
						}
					}

					m_ContinuousInfo.hasInputTime += dt;
					m_ContinuousInfo.noInputTime = 0.0f;
					m_ContinuousInfo.previousInput = moveInput;
				}
				else
				{
					m_ContinuousInfo.noInputTime += dt;
					m_ContinuousInfo.hasInputTime = 0.0f;
				}
			}

			/// <summary>
			/// Determine if the modifier's state should change.
			/// </summary>
			void CheckIfStateChanges()
			{
				switch (state)
				{
					case ModifierState.Idle:
					{
						if (ShouldChangeToState(ModifierState.RotateInCircle))
						{
							SetState(ModifierState.RotateInCircle);
						}
						else if (ShouldChangeToState(ModifierState.MoveForward))
						{
							SetState(ModifierState.MoveForward);
						}

						break;
					}
					case ModifierState.MoveForward:
					{
						if (ShouldChangeToState(ModifierState.RotateInCircle))
						{
							SetState(ModifierState.RotateInCircle);
						}
						else if (ShouldChangeToState(ModifierState.Idle))
						{
							SetState(ModifierState.Idle);
						}

						break;
					}
					case ModifierState.RotateInCircle:
					{
						if (ShouldChangeToState(ModifierState.MoveForward))
						{
							SetState(ModifierState.MoveForward);
						}
						else if (ShouldChangeToState(ModifierState.Idle))
						{
							SetState(ModifierState.Idle);
						}

						break;
					}
				}
			}

			/// <summary>
			/// Test if the modifier state should change to the specified state.
			/// </summary>
			bool ShouldChangeToState(ModifierState stateToTest)
			{
				switch (stateToTest)
				{
					case ModifierState.Idle:
					{
						return !m_HasInput &&
						       m_ContinuousInfo.noInputTime > m_ChangeIdleDelay;
					}
					case ModifierState.MoveForward:
					{
						if (state == ModifierState.Idle)
						{
							// At this point the RotateInCircle check failed (in CheckIfStateChanges), so we can safely move forward
							return m_HasInput &&
							       m_ContinuousInfo.hasInputTime > m_ChangeIdleToMoveDelay;
						}

						return m_HasInput &&
						       m_ContinuousInfo.smallAngleTime > m_ChangeRotateToMoveDelay &&
						       Mathf.Abs(m_ContinuousInfo.averageInputAngle) < m_StartMoveMaxAngle;
					}
					case ModifierState.RotateInCircle:
					{
						if (m_LastRawInput.magnitude < k_MinInputMagnitudeForRotateInCircle)
						{
							return false;
						}
						var rotatedLongOrFarEnough =
							m_ContinuousInfo.averageInputDirectionTime > m_StartRotateInCircleMinTime ||
							m_ContinuousInfo.averageInputRotatingTotalAngle > m_StartRotateInCircleMinTotalAngle;

						if (m_HasInput &&
						    Mathf.Abs(m_ContinuousInfo.averageInputAngle) >= m_StartRotateInCircleMinAngle &&
						    !Mathf.Approximately(m_ContinuousInfo.averageInputDirection, 0.0f))
						{
							// If we are close to rotating in a circle then disable rapid turn
							var angle = m_IsLowFrameRate
								              ? m_DisableRapidTurnBeforeRotateMinAngleLowFrameRate
								              : m_DisableRapidTurnBeforeRotateMinAngle;
							if (m_ContinuousInfo.averageInputDirectionTime > m_DisableRapidTurnBeforeRotateMinTime ||
							    m_ContinuousInfo.averageInputRotatingTotalAngle > angle)
							{
								m_Motor.DisableRapidTurn(this);
							}

							return rotatedLongOrFarEnough;
						}

						break;
					}
				}

				return false;
			}

			void UpdateState(Vector2 moveInput)
			{
				switch (state)
				{
					case ModifierState.MoveForward:
					{
						UpdateStateMoveForward(moveInput);
						break;
					}
					case ModifierState.RotateInCircle:
					{
						UpdateStateRotateInCircle(moveInput);
						break;
					}
				}
			}

			void UpdateStateMoveForward(Vector2 moveInput)
			{
				if (m_CatchUpToInputAfterRotating)
				{
					m_TargetMoveInput = moveInput;
					m_TargetDirection = -m_PreviousRotateInCircleDirection;
				}
			}

			/// <summary>
			/// Rotate the character in the direction in which the input is rotating.
			/// </summary>
			void UpdateStateRotateInCircle(Vector2 moveInput)
			{
				// Direction changed?
				if (!Mathf.Approximately(rotateInCircleDirection, m_ContinuousInfo.averageInputDirection) &&
				    !Mathf.Approximately(m_ContinuousInfo.averageInputDirection, 0.0f))
				{
					rotateInCircleDirection = m_ContinuousInfo.averageInputDirection;
				}

				// Note: Vector2.SignedAngle gives an opposite sign to what is needed for 3D rotation, so we
				// negate rotateInCircleDirection
				var rotateDirection = Mathf.Sign(-rotateInCircleDirection);

				m_TargetMoveInput =
					CalculateRotationFromCharacterForward(rotateDirection, m_MaxRotateAngle, moveInput.magnitude);
				m_TargetDirection = rotateDirection;
			}

			/// <summary>
			/// Rotate the character's forward vector towards the target vector (if any).
			/// </summary>
			void UpdateRotation(ref Vector2 moveInput)
			{
				if (m_TargetMoveInput == null)
				{
					return;
				}

				m_Motor.DisableRapidTurn(this);

				// Note: We don't use delta time for the rotation, because it may get out of sync with the
				// character's forward vector. Which will cause jerky movement. The character itself will rotate smoothly 
				// towards the input vector.

				var length = m_TargetMoveInput.Value.magnitude;
				var from = new Vector3(m_CharacterForwardInput.x, 0.0f, m_CharacterForwardInput.y);
				var target = new Vector3(m_TargetMoveInput.Value.x, 0.0f, m_TargetMoveInput.Value.y);
				var signedAngle = Vector3.SignedAngle(from, target, Vector3.up);
				var rotateAngle = Mathf.Min(Mathf.Abs(signedAngle), m_MaxRotateAngle);
				var rotateDirection = !Mathf.Approximately(m_TargetDirection, 0.0f)
					                        ? m_TargetDirection
					                        : Mathf.Sign(signedAngle);
				var rotated = Quaternion.Euler(0.0f, rotateAngle * rotateDirection, 0.0f) * from;

				if (m_CatchUpToInputAfterRotating)
				{
					// Reached target?
					if (Vector3.Angle(target, rotated) < k_StopCatchAfterRotateAngle)
					{
						m_CatchUpToInputAfterRotating = false;
					}
				}

				moveInput = new Vector2(rotated.x, rotated.z).normalized * length;
			}

			/// <summary>
			/// Calculate an input vector rotated from the character's forward input vector.
			/// </summary>
			/// <remarks>
			/// The character's forward vector may not be 100% correct, because it's dependent on the camera. So avoid using  
			/// small values for rotateAngle (i.e. should be at least 5 degrees).
			/// </remarks>
			/// <param name="rotateDirection">The direction to rotate.</param>
			/// <param name="rotateAngle">The angle to rotate (degrees).</param>
			/// <param name="length">Required length of the vector.</param>
			Vector2 CalculateRotationFromCharacterForward(float rotateDirection, float rotateAngle,
			                                                      float length)
			{
				var forward = new Vector3(m_CharacterForwardInput.x, 0.0f, m_CharacterForwardInput.y);
				var forwardTarget = Quaternion.Euler(0.0f, rotateAngle * rotateDirection, 0.0f) * forward;
				return new Vector2(forwardTarget.x, forwardTarget.z).normalized * length;
			}

			/// <summary>
			/// Calculate the angle between the character's forward input and the move input.
			/// </summary>
			float CalculateAngleBetweenInputAndCharacterForward(Vector2 moveInput)
			{
				var from = new Vector3(m_CharacterForwardInput.x, 0.0f, m_CharacterForwardInput.y);
				var to = new Vector3(moveInput.x, 0.0f, moveInput.y);
				return Vector3.Angle(from, to);
			}

#if UNITY_EDITOR
			void OnValidate()
			{
				//Design pattern for fetching required scene references
				FindBrain();
			}

			void Reset()
			{
				//Design pattern for fetching required scene references
				FindBrain();
			}

			/// <summary>
			/// DEBUG: Draw the input vectors.
			/// </summary>
			/// <param name="moveInput">The moveInput. It may be modified by this point.</param>
			/// <param name="snapShotType">0 = draw for a single frame, 1 = draw for longer duration, 2 = draw for longer
			/// duration with less alpha.</param>
			void DebugUpdate(Vector2 moveInput, int snapShotType = 0)
			{
				if (m_CharacterTransform == null)
				{
					return;
				}
	
				var offsetY = m_DebugOffsetY;
				Color color;
				var duration = snapShotType != 0 ? 10.0f : 0.0f;
				var alphaMultiplier = snapShotType != 2 ? 1.0f : 0.5f;
	
				if (m_DebugShowValidInput)
				{
					color = m_DebugValidInputColor;
					color.a *= alphaMultiplier;
					if (state == ModifierState.RotateInCircle)
					{
						DebugDrawInput(m_ValidMoveInput, offsetY, color, duration, -rotateInCircleDirection, 1.1f);
					}
					else
					{
						DebugDrawInput(m_ValidMoveInput, offsetY, color, duration, null, 1.1f);
					}
				}
	
				if (m_DebugShowModifiedInput &&
					state == ModifierState.RotateInCircle ||
					m_CatchUpToInputAfterRotating)
				{
					color = m_DebugShowModifiedInputColor;
					color.a *= alphaMultiplier;
					if (m_CatchUpToInputAfterRotating)
					{
						// "Thick" line
						for (var i = 0; i < 4; i++)
						{
							DebugDrawInput(moveInput, offsetY + (0.01f * i), color, duration, -m_PreviousRotateInCircleDirection, 1.5f);
						}
					}
					else
					{
						DebugDrawInput(moveInput, offsetY, color, duration, -rotateInCircleDirection);
					}
				}
				
				// Character's forward vector
				if (m_DebugShowCharacterForward)
				{
					color = m_DebugCharacterForwardColor;
					color.a *= alphaMultiplier;
					DebugDrawInput(new Vector2(m_CharacterTransform.forward.x, m_CharacterTransform.forward.z), 
								   offsetY, color, duration);
				}
	
				if (m_DebugShowCharacterForwardInput)
				{
					color = m_DebugShowCharacterForwardInputColor;
					color.a *= alphaMultiplier;
					DebugDrawInput(m_CharacterForwardInput, offsetY, color, duration);
				}
			}
	
			/// <summary>
			/// DEBUG: Draw the input vector.
			/// </summary>
			void DebugDrawInput(Vector2 input, float offsetY, Color color, float duration,
										float? rotateDirection = null,
										float scale = 1.0f)
			{
				var vector = new Vector3(input.x, 0.0f, input.y);
				var point = m_CharacterTransform.position + new Vector3(0.0f, offsetY, 0.0f);
				if (m_UnityCameraTransform != null)
				{
					vector = m_UnityCameraTransform.TransformVector(vector);
					
					// Project onto the ground
					vector = Vector3.ProjectOnPlane(vector, Vector3.up);
				}
				
				vector = vector.normalized * scale;
				
				Debug.DrawRay(point, 
							  vector, 
							  color, 
							  duration);
	
				if (rotateDirection != null &&
					!Mathf.Approximately(rotateDirection.Value, 0.0f))
				{
					var sideLength = 0.2f;
					Vector3 side;
					if (rotateDirection > 0.0f)
					{
						side = Quaternion.AngleAxis(90.0f, Vector3.up) * vector;
					}
					else
					{
						side = Quaternion.AngleAxis(-90.0f, Vector3.up) * vector;
					}
					side.Normalize();
					Debug.DrawRay(point + (vector * 0.5f), 
								  side * sideLength, 
								  color, 
								  duration);
				}
			}
#endif
		}
	}
}
using System;
using StandardAssets.Characters.Common;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Implementation of the Third Person input
	/// </summary>
	public class ThirdPersonInput : CharacterInput, IThirdPersonInput
	{
		/// <summary>
		/// Smooths the input movement when rotating in fast circles. This makes the character run in a circle, instead of turning around on the spot.
		/// </summary>
		[SerializeField, Tooltip("Smooths the input movement when rotating in fast circles. This makes the character run in a circle, instead of turning around on the spot.")]
		protected CharacterInputModifier locomotionInputSmoother;

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
		protected bool isStrafing;

		/// <summary>
		/// Conditions the <paramref name="rawMoveInput"/>
		/// </summary>
		/// <param name="rawMoveInput">The move input vector received from the input action</param>
		/// <returns>The input vector conditioned by <see cref="CharacterInputModifier"/></returns>
		protected override Vector2 ConditionMoveInput(Vector2 rawMoveInput)
		{
			if (locomotionInputSmoother != null)
			{
				locomotionInputSmoother.OnGotRawInput(ref rawMoveInput);
			}
			return rawMoveInput;
		}

		/// <summary>
		/// Registers strafe and recentre inputs.
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
			controls.Movement.strafe.performed += OnStrafeInput;
			controls.Movement.recentre.performed += OnRecentreInput;
		}

		/// <summary>
		/// Handles the recentre input 
		/// </summary>
		/// <param name="context">context is required by the performed event</param>
		private void OnRecentreInput(InputAction.CallbackContext context)
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
		private void OnStrafeInput(InputAction.CallbackContext context)
		{
			BroadcastInputAction(ref isStrafing, strafeStarted, strafeEnded);
		}

		/// <summary>
		/// Initializes the movement input modifier.
		/// </summary>
		private void Start()
		{
			locomotionInputSmoother.Init();
		}

		/// <summary>
		/// Call the modifier's OnDisable.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			if (locomotionInputSmoother != null)
			{
				locomotionInputSmoother.OnDisable();
			}
		}

		/// <summary>
		/// Use the modifier to update the move input.
		/// </summary>
		protected override void Update()
		{
			base.Update();
			if (locomotionInputSmoother != null)
			{
				moveInput = locomotionInputSmoother.UpdateMoveInput();
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
			private const float k_LowFrameRateFramesPerSecond = 35.0f;

			/// <summary>
			/// Low frame rate delta time (seconds).
			/// </summary>
			private const float k_LowFrameRateDeltaTime = 1.0f / k_LowFrameRateFramesPerSecond;

			/// <summary>
			/// Reset the average samples after this time (seconds).
			/// </summary>
			private const float k_AverageSamplesMinTime = 0.5f;

			/// <summary>
			/// Reset the average samples after this amount of samples.
			/// </summary>
			private const int k_AverageMinSamples = 5;

			/// <summary>
			/// Stop catching up to the input vector when the angle to the vector is less than this (degrees). Must be
			/// greater than zero.
			/// </summary>
			private const float k_StopCatchAfterRotateAngle = 0.1f;

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
			[Header("Character")]
			[SerializeField, Tooltip("The third person character brain.")]
			private ThirdPersonBrain characterBrain;

			/// <summary>
			/// Delay before changing to Idle (seconds).
			/// </summary>
			[Header("Changing States")]
			[SerializeField, Tooltip("Delay before changing to Idle (seconds).")]
			private float changeIdleDelay = 0.1f;

			/// <summary>
			/// Delay before changing from Idle to MoveForward (seconds).
			/// </summary>
			[SerializeField, Tooltip("Delay before changing from Idle to MoveForward (seconds).")]
			private float changeIdleToMoveDelay = 0.1f;

			/// <summary>
			/// Delay before changing from RotateInCircle to MoveForward (seconds).
			/// </summary>
			[SerializeField, Tooltip("Delay before changing from RotateInCircle to MoveForward (seconds).")]
			private float changeRotateToMoveDelay = 0.1f;

			/// <summary>
			/// Start MoveForward when the change in direction's angle is less than this (degrees).
			/// </summary>
			[SerializeField,
			 Tooltip("Start MoveForward when the change in direction's angle is less than this (degrees).")]
			private float startMoveMaxAngle = 1.0f;

			/// <summary>
			/// Start RotateInCircle after rotating input for this amount of time (seconds).
			/// </summary>
			[SerializeField, Tooltip("Start RotateInCircle after rotating input for this amount of time (seconds).")]
			private float startRotateInCircleMinTime = 1.5f;

			/// <summary>
			/// Start RotateInCircle after rotating input for this amount of degrees.
			/// </summary>
			[SerializeField, Tooltip("Start RotateInCircle after rotating input for this amount of degrees.")]
			private float startRotateInCircleMinTotalAngle = 270.0f;

			/// <summary>
			/// Start RotateInCircle if the change in direction's angle is more than this (degrees). Must be bigger
			/// than startMoveMaxAngle.
			/// </summary>
			[SerializeField, Tooltip(
				 "Start RotateInCircle if the change in direction's angle is more than this (degrees). " +
				 "Must be bigger than startMoveMaxAngle.")]
			private float startRotateInCircleMinAngle = 2.0f;

			/// <summary>
			/// Max angle at which to rotate the character's forward vector towards the target vector (degrees). Do not make
			/// this too small, because calculating the player's forward input vector is not 100% accurate.
			/// </summary>
			[Header("Angles")]
			[SerializeField, Tooltip(
				 "Max angle at which to rotate the character's forward vector towards the target vector " +
				 "(degrees). Do not make this too small, because calculating the player's forward input " +
				 "vector is not 100% accurate.")]
			private float maxRotateAngle = 20.0f;

			/// <summary>
			/// When RotateInCircle ends, if angle between character's forward and current input is greater than this,
			/// then rotate to the input vector.
			/// </summary>
			[SerializeField, Tooltip("When RotateInCircle ends, if angle between character's forward and current " +
			                         "input is greater than this, then rotate to the input vector.")]
			private float catchUpToInputMinAngle = 90.0f;

			/// <summary>
			/// Disable rapid turn before starting RotateInCircle after rotating input for this amount of time (seconds).
			/// Must be less than startRotateInCircleMinTime.
			/// </summary>
			[Header("Disable Rapid Turn")]
			[SerializeField, Tooltip(
				 "Disable rapid turn before starting RotateInCircle after rotating input for this " +
				 "amount of time (seconds). Must be less than startRotateInCircleMinTime.")]
			private float disableRapidTurnBeforeRotateMinTime = 1.0f;

			/// <summary>
			/// Disable rapid turn before starting RotateInCircle after rotating input for this amount of degrees. Must
			/// be less than startRotateInCircleMinTotalAngle.
			/// </summary>
			[SerializeField, Tooltip(
				 "Disable rapid turn before starting RotateInCircle after rotating input for this amount " +
				 "of degrees. Must be less than startRotateInCircleMinTotalAngle.")]
			private float disableRapidTurnBeforeRotateMinAngle = 180.0f;

			/// <summary>
			/// Disable rapid turn before starting RotateInCircle after rotating input for this amount of degrees (when
			/// frame rate is low). Must be less than startRotateInCircleMinTotalAngle.
			/// </summary>
			[SerializeField, Tooltip(
				 "Disable rapid turn before starting RotateInCircle after rotating input for this amount " +
				 "of degrees (when frame rate is low). Must be less than startRotateInCircleMinTotalAngle.")]
			private float disableRapidTurnBeforeRotateMinAngleLowFrameRate = 90.0f;

			/// <summary>
			/// Enable debug in the editor?
			/// </summary>
			[Header("Debug")]
			[SerializeField, Tooltip("Enable debug in the editor?")]
			private bool debugEnabled;

			[SerializeField, Tooltip("Draw vectors this Y offset from the character.")]
			private float debugOffsetY;

			/// <summary>
			/// Character's transform
			/// </summary>
			private Transform characterTransform;

			/// <summary>
			/// The root motion motor.
			/// </summary>
			private ThirdPersonMotor motor;

			/// <summary>
			/// The character controller controllerAdapter.
			/// </summary>
			private ControllerAdapter controllerAdapter;

			/// <summary>
			/// The camera to use for transforming input. It can be changed via SetCamera.
			/// </summary>
			private Camera unityCamera;

			/// <summary>
			/// Transform of the camera.
			/// </summary>
			private Transform unityCameraTransform;

			/// <summary>
			/// Was disabled in the previous update?
			/// </summary>
			private bool wasDisabled;

			/// <summary>
			/// Does the current update have input?
			/// </summary>
			private bool hasInput;

			/// <summary>
			/// The last received raw input.
			/// </summary>
			private Vector2 lastRawInput;

			/// <summary>
			/// Last real time since startup.
			/// </summary>
			private float lastRealTimeSinceStartup;

			/// <summary>
			/// The last non-zero-length move input vector.
			/// </summary>
			private Vector2 validMoveInput;

			/// <summary>
			/// The character's forward vector converted to input.
			/// </summary>
			private Vector2 characterForwardInput;

			/// <summary>
			/// Rotate the character's forward vector to this target input vector.
			/// </summary>
			private Vector2? targetMoveInput;

			/// <summary>
			/// Direction to rotate when rotating the character's forward vector to the target input vector.
			/// </summary>
			private float targetDirection;

			/// <summary>
			/// Catching up to the input vector after rotating in a circle?
			/// </summary>
			private bool catchUpToInputAfterRotating;

			/// <summary>
			/// Is the frame rate low? We handle some special cases for low frame rates.
			/// </summary>
			private bool isLowFrameRate;

			/// <summary>
			/// Continuous input info.
			/// </summary>
			private readonly ContinuousInfo continuousInfo = new ContinuousInfo();

			/// <summary>
			/// Previous value of rotateInCircleDirection.
			/// </summary>
			private float previousRotateInCircleDirection;

#if UNITY_EDITOR
			private readonly bool debugShowValidInput = true;
			private readonly Color debugValidInputColor = Color.green;
			private readonly bool debugShowModifiedInput = true;
			private readonly Color debugShowModifiedInputColor = Color.red;
			private readonly bool debugShowCharacterForward = true;
			private readonly Color debugCharacterForwardColor = Color.blue;
			private readonly bool debugShowCharacterForwardInput = true;
			private readonly Color debugShowCharacterForwardInputColor = Color.white;
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
				lastRawInput = rawInput;
				ModifyMoveInput(ref rawInput);
			}

			/// <summary>
			/// Update the move input, based on the last raw input that was received. This simulates a polling system.
			/// </summary>
			public Vector2 UpdateMoveInput()
			{
				Vector2 moveInput = lastRawInput;
				ModifyMoveInput(ref moveInput);
				return moveInput;
			}
			
			/// <summary>
			/// Modify the move input.
			/// </summary>
			public void ModifyMoveInput(ref Vector2 moveInput)
			{
				motor.EnableRapidTurn(this);

				// Only modify when enabled, character is grounded, and not straffing
				if (characterTransform == null || 
				    !characterTransform.gameObject.activeInHierarchy || 
				    !controllerAdapter.isGrounded || 
				    motor.movementMode == ThirdPersonMotorMovementMode.Strafe)
				{
					wasDisabled = true;
					return;
				}

				float dt = Time.realtimeSinceStartup - lastRealTimeSinceStartup;
				lastRealTimeSinceStartup = Time.realtimeSinceStartup;
				hasInput = moveInput.sqrMagnitude > 0.0f;
				isLowFrameRate = dt > k_LowFrameRateDeltaTime;

				if (wasDisabled)
				{
					wasDisabled = false;
					SetState(ModifierState.Idle);
				}

				if (hasInput)
				{
					validMoveInput = moveInput;
				}

				targetMoveInput = null;
				targetDirection = 0.0f;
				characterForwardInput = CalculateCharacterForwardInput();

				UpdateContinuousInfo(moveInput, dt);
				CheckIfStateChanges();
				UpdateState(moveInput);
				UpdateRotation(ref moveInput);

#if UNITY_EDITOR
				if (debugEnabled)
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
				unityCamera = newCamera;
				if (unityCamera != null)
				{
					unityCameraTransform = unityCamera.transform;
				}
			}

			public void Init()
			{
				FindBrain();
				characterTransform = characterBrain.transform;
				motor = characterBrain.thirdPersonMotor;
				controllerAdapter = characterBrain.controllerAdapter;
				
				if (unityCamera == null)
				{
					// Use the main camera as the default. The camera can be changed later via SetCamera.
					SetCamera(Camera.main);
					if (unityCamera == null)
					{
						SetCamera(GameObject.FindObjectOfType<Camera>());
					}
				}

				lastRealTimeSinceStartup = Time.realtimeSinceStartup;
				SetState(ModifierState.Idle);
			}

			public void OnDisable()
			{
				wasDisabled = true;

				if (motor != null)
				{
					motor.EnableRapidTurn(this);
				}
			}

			/// <summary>
			/// Calculate the character's forward vector as an input vector.
			/// </summary>
			/// <returns></returns>
			private Vector2 CalculateCharacterForwardInput()
			{
				Vector3 direction = characterTransform.forward;

				// Ignore height
				direction.y = 0.0f;
				direction.Normalize();

				// To local, relative to camera
				direction = unityCameraTransform.InverseTransformDirection(direction);

				return new Vector2(direction.x, direction.z).normalized;
			}

			/// <summary>
			/// Helper for checking if the <see cref="ThirdPersonBrain"/> has been assigned - otherwise looks for it in the scene
			/// </summary>
			private void FindBrain()
			{
				if (characterBrain == null)
				{
					Debug.Log("No ThirdPersonBrain setup - using FindObjectOfType");
					ThirdPersonBrain[] brainsInScene = GameObject.FindObjectsOfType<ThirdPersonBrain>();
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

					characterBrain = brainsInScene[0];
				}
			}

			/// <summary>
			/// Set the modifier's state.
			/// </summary>
			private void SetState(ModifierState newState)
			{
				ModifierState oldState = state;
				state = newState;

				if (oldState == ModifierState.RotateInCircle)
				{
					previousRotateInCircleDirection = rotateInCircleDirection;
				}

				switch (state)
				{
					case ModifierState.Idle:
					{
						continuousInfo.Clear();
						catchUpToInputAfterRotating = false;

						break;
					}
					case ModifierState.MoveForward:
					{
						continuousInfo.averageInputRotatingVector = Vector2.zero;
						continuousInfo.averageInputRotatingTotalAngle = 0.0f;

						if (oldState == ModifierState.RotateInCircle &&
						    CalculateAngleBetweenInputAndCharacterForward(validMoveInput) > catchUpToInputMinAngle)
						{
							catchUpToInputAfterRotating = true;
						}

						break;
					}
					case ModifierState.RotateInCircle:
					{
						continuousInfo.smallAngleTime = 0.0f;
						rotateInCircleDirection = continuousInfo.averageInputDirection;
						catchUpToInputAfterRotating = false;

						break;
					}
				}
			}

			/// <summary>
			/// Update the continous input info, which is used to help determine the modifier's state.
			/// </summary>
			private void UpdateContinuousInfo(Vector2 moveInput, float dt)
			{
				if (hasInput)
				{
					if (continuousInfo.hasInputTime > 0.0f)
					{
						// Angle between current and previous input
						float angle = Vector2.SignedAngle(continuousInfo.previousInput, moveInput);
						float direction = !Mathf.Approximately(angle, 0.0f)
							                  ? Mathf.Sign(angle)
							                  : 0.0f;
						// Rotating in the same direction?
						if (Mathf.Approximately(continuousInfo.inputDirection, direction))
						{
							// Note: This also takes into account a direction of zero, which means input is not rotating.
							continuousInfo.inputDirectionTime += dt;
						}
						else
						{
							continuousInfo.inputDirectionTime = dt;
						}

						continuousInfo.inputAngle = angle;
						continuousInfo.inputDirection = direction;

						// Calculate averages
						continuousInfo.averageInputAngleTotal += angle;
						continuousInfo.averageSamples++;
						continuousInfo.averageTime += dt;
						continuousInfo.averageInputAngle = continuousInfo.averageInputAngleTotal /
						                                   continuousInfo.averageSamples;
						continuousInfo.averageInputDirection =
							!Mathf.Approximately(continuousInfo.averageInputAngle, 0.0f)
								? Mathf.Sign(continuousInfo.averageInputAngle)
								: 0.0f;

						if (continuousInfo.averageTime > k_AverageSamplesMinTime &&
						    continuousInfo.averageSamples > k_AverageMinSamples)
						{
							continuousInfo.averageInputAngleTotal = 0.0f;
							continuousInfo.averageSamples = 0;
							continuousInfo.averageTime = 0.0f;
						}

						// Average rotating in the same direction?
						if (Mathf.Approximately(continuousInfo.averageInputDirection,
						                        continuousInfo.previousAverageInputDirection))
						{
							// Note: This also takes into account a direction of zero, which means input is not rotating.
							continuousInfo.averageInputDirectionTime += dt;
							continuousInfo.averageInputRotatingTotalAngle +=
								Vector2.Angle(moveInput, continuousInfo.averageInputRotatingVector);
						}
						else
						{
							continuousInfo.averageInputDirectionTime = dt;
							continuousInfo.averageInputRotatingTotalAngle = 0.0f;
						}

						continuousInfo.previousAverageInputDirection = continuousInfo.averageInputDirection;
						continuousInfo.averageInputRotatingVector = moveInput;

						if (Mathf.Abs(continuousInfo.averageInputAngle) < startMoveMaxAngle)
						{
							continuousInfo.smallAngleTime += dt;
						}
						else
						{
							continuousInfo.smallAngleTime = 0.0f;
						}
					}

					continuousInfo.hasInputTime += dt;
					continuousInfo.noInputTime = 0.0f;
					continuousInfo.previousInput = moveInput;
				}
				else
				{
					continuousInfo.noInputTime += dt;
					continuousInfo.hasInputTime = 0.0f;
				}
			}

			/// <summary>
			/// Determine if the modifier's state should change.
			/// </summary>
			private void CheckIfStateChanges()
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
			private bool ShouldChangeToState(ModifierState stateToTest)
			{
				switch (stateToTest)
				{
					case ModifierState.Idle:
					{
						return !hasInput &&
						       continuousInfo.noInputTime > changeIdleDelay;
					}
					case ModifierState.MoveForward:
					{
						if (state == ModifierState.Idle)
						{
							// At this point the RotateInCircle check failed (in CheckIfStateChanges), so we can safely move forward
							return hasInput &&
							       continuousInfo.hasInputTime > changeIdleToMoveDelay;
						}

						return hasInput &&
						       continuousInfo.smallAngleTime > changeRotateToMoveDelay &&
						       Mathf.Abs(continuousInfo.averageInputAngle) < startMoveMaxAngle;
					}
					case ModifierState.RotateInCircle:
					{
						bool rotatedLongOrFarEnough =
							continuousInfo.averageInputDirectionTime > startRotateInCircleMinTime ||
							continuousInfo.averageInputRotatingTotalAngle > startRotateInCircleMinTotalAngle;

						if (hasInput &&
						    Mathf.Abs(continuousInfo.averageInputAngle) >= startRotateInCircleMinAngle &&
						    !Mathf.Approximately(continuousInfo.averageInputDirection, 0.0f))
						{
							// If we are close to rotating in a circle then disable rapid turn
							float angle = isLowFrameRate
								              ? disableRapidTurnBeforeRotateMinAngleLowFrameRate
								              : disableRapidTurnBeforeRotateMinAngle;
							if (continuousInfo.averageInputDirectionTime > disableRapidTurnBeforeRotateMinTime ||
							    continuousInfo.averageInputRotatingTotalAngle > angle)
							{
								motor.DisableRapidTurn(this);
							}

							return rotatedLongOrFarEnough;
						}

						break;
					}
				}

				return false;
			}

			private void UpdateState(Vector2 moveInput)
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

			private void UpdateStateMoveForward(Vector2 moveInput)
			{
				if (catchUpToInputAfterRotating)
				{
					targetMoveInput = moveInput;
					targetDirection = -previousRotateInCircleDirection;
				}
			}

			/// <summary>
			/// Rotate the character in the direction in which the input is rotating.
			/// </summary>
			private void UpdateStateRotateInCircle(Vector2 moveInput)
			{
				// Direction changed?
				if (!Mathf.Approximately(rotateInCircleDirection, continuousInfo.averageInputDirection) &&
				    !Mathf.Approximately(continuousInfo.averageInputDirection, 0.0f))
				{
					rotateInCircleDirection = continuousInfo.averageInputDirection;
				}

				// Note: Vector2.SignedAngle gives an opposite sign to what is needed for 3D rotation, so we
				// negate rotateInCircleDirection
				float rotateDirection = Mathf.Sign(-rotateInCircleDirection);

				targetMoveInput =
					CalculateRotationFromCharacterForward(rotateDirection, maxRotateAngle, moveInput.magnitude);
				targetDirection = rotateDirection;
			}

			/// <summary>
			/// Rotate the character's forward vector towards the target vector (if any).
			/// </summary>
			private void UpdateRotation(ref Vector2 moveInput)
			{
				if (targetMoveInput == null)
				{
					return;
				}

				motor.DisableRapidTurn(this);

				// Note: We don't use delta time for the rotation, because it may get out of sync with the
				// character's forward vector. Which will cause jerky movement. The character itself will rotate smoothly 
				// towards the input vector.

				float length = targetMoveInput.Value.magnitude;
				Vector3 from = new Vector3(characterForwardInput.x, 0.0f, characterForwardInput.y);
				Vector3 target = new Vector3(targetMoveInput.Value.x, 0.0f, targetMoveInput.Value.y);
				float signedAngle = Vector3.SignedAngle(from, target, Vector3.up);
				float rotateAngle = Mathf.Min(Mathf.Abs(signedAngle), maxRotateAngle);
				float rotateDirection = !Mathf.Approximately(targetDirection, 0.0f)
					                        ? targetDirection
					                        : Mathf.Sign(signedAngle);
				Vector3 rotated = Quaternion.Euler(0.0f, rotateAngle * rotateDirection, 0.0f) * from;

				if (catchUpToInputAfterRotating)
				{
					// Reached target?
					if (Vector3.Angle(target, rotated) < k_StopCatchAfterRotateAngle)
					{
						catchUpToInputAfterRotating = false;
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
			private Vector2 CalculateRotationFromCharacterForward(float rotateDirection, float rotateAngle,
			                                                      float length)
			{
				Vector3 forward = new Vector3(characterForwardInput.x, 0.0f, characterForwardInput.y);
				Vector3 forwardTarget = Quaternion.Euler(0.0f, rotateAngle * rotateDirection, 0.0f) * forward;
				return new Vector2(forwardTarget.x, forwardTarget.z).normalized * length;
			}

			/// <summary>
			/// Calculate the angle between the character's forward input and the move input.
			/// </summary>
			private float CalculateAngleBetweenInputAndCharacterForward(Vector2 moveInput)
			{
				Vector3 from = new Vector3(characterForwardInput.x, 0.0f, characterForwardInput.y);
				Vector3 to = new Vector3(moveInput.x, 0.0f, moveInput.y);
				return Vector3.Angle(from, to);
			}

#if UNITY_EDITOR
			/// <summary>
			/// DEBUG: Draw the input vectors.
			/// </summary>
			/// <param name="moveInput">The moveInput. It may be modified by this point.</param>
			/// <param name="snapShotType">0 = draw for a single frame, 1 = draw for longer duration, 2 = draw for longer
			/// duration with less alpha.</param>
			private void DebugUpdate(Vector2 moveInput, int snapShotType = 0)
			{
				if (characterTransform == null)
				{
					return;
				}
	
				float offsetY = debugOffsetY;
				Color color;
				float duration = snapShotType != 0 ? 10.0f : 0.0f;
				float alphaMultiplier = snapShotType != 2 ? 1.0f : 0.5f;
	
				if (debugShowValidInput)
				{
					color = debugValidInputColor;
					color.a *= alphaMultiplier;
					if (state == ModifierState.RotateInCircle)
					{
						DebugDrawInput(validMoveInput, offsetY, color, duration, -rotateInCircleDirection, 1.1f);
					}
					else
					{
						DebugDrawInput(validMoveInput, offsetY, color, duration, null, 1.1f);
					}
				}
	
				if (debugShowModifiedInput &&
					state == ModifierState.RotateInCircle ||
					catchUpToInputAfterRotating)
				{
					color = debugShowModifiedInputColor;
					color.a *= alphaMultiplier;
					if (catchUpToInputAfterRotating)
					{
						// "Thick" line
						for (int i = 0; i < 4; i++)
						{
							DebugDrawInput(moveInput, offsetY + (0.01f * i), color, duration, -previousRotateInCircleDirection, 1.5f);
						}
					}
					else
					{
						DebugDrawInput(moveInput, offsetY, color, duration, -rotateInCircleDirection);
					}
				}
				
				// Character's forward vector
				if (debugShowCharacterForward)
				{
					color = debugCharacterForwardColor;
					color.a *= alphaMultiplier;
					DebugDrawInput(new Vector2(characterTransform.forward.x, characterTransform.forward.z), 
								   offsetY, color, duration);
				}
	
				if (debugShowCharacterForwardInput)
				{
					color = debugShowCharacterForwardInputColor;
					color.a *= alphaMultiplier;
					DebugDrawInput(characterForwardInput, offsetY, color, duration);
				}
			}
	
			/// <summary>
			/// DEBUG: Draw the input vector.
			/// </summary>
			private void DebugDrawInput(Vector2 input, float offsetY, Color color, float duration,
										float? rotateDirection = null,
										float scale = 1.0f)
			{
				Vector3 vector = new Vector3(input.x, 0.0f, input.y);
				Vector3 point = characterTransform.position + new Vector3(0.0f, offsetY, 0.0f);
				if (unityCameraTransform != null)
				{
					vector = unityCameraTransform.TransformVector(vector);
					
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
					float sideLength = 0.2f;
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
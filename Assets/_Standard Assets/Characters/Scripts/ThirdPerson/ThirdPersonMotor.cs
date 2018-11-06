using System;
using System.Collections.Generic;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Helpers;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// IThirdPersonMotor implementation that uses, primarily, root motion from the animator to move the character.
	/// </summary>
	[Serializable]
	public class ThirdPersonMotor
	{
		/// <summary>
		/// Track distance above the ground at these frame intervals (to prevent checking every frame)
		/// </summary>
		private const int k_TrackGroundFrameIntervals = 5;
		
		/// <summary>
		/// Various configuration settings more movement.
		/// </summary>
		[SerializeField, Tooltip("Reference to the configuration with all the movement settings")]
		protected MotorConfig configuration;

		/// <summary>
		/// Gets the normalized turning speed
		/// </summary>
		/// <value>A value normalized using turning settings from <see cref="configuration"/> </value>
		public float normalizedTurningSpeed { get; private set; }
		
		/// <summary>
		/// Gets the normalized lateral speed
		/// </summary>
		/// <value>A value normalized using lateral settings from <see cref="configuration"/> </value>
		public float normalizedLateralSpeed { get; private set; }
		
		/// <summary>
		/// Gets the normalized forward speed
		/// </summary>
		/// <value>A value normalized using forward settings from <see cref="configuration"/> </value>
		public float normalizedForwardSpeed { get; private set; }

		/// <summary>
		/// Gets the character's current fall time.
		/// </summary>
		/// <value>The time, in seconds, the character has been in a falling state.</value>
		public float fallTime
		{
			get { return controllerAdapter.fallTime; }
		}

		/// <summary>
		/// Gets the current target y rotation.
		/// </summary>
		/// <value>The current y rotation, in degrees.</value>
		public float targetYRotation { get; private set; }

		/// <summary>
		/// Gets the velocity that was cached as the character exited a root motion state.
		/// </summary>
		/// <value>An moving average of the root motion velocity.</value>
		public float cachedForwardVelocity { get; private set; }
		
		/// <summary>
		/// Gets the character's current movement mode.
		/// </summary>
		/// <value>Either Action or Strafe.</value>
		public ThirdPersonMotorMovementMode movementMode { get; private set; }

		/// <summary>
		/// Is rapid turn disabled? (Enable/Disable it via <see cref="EnableRapidTurn"/>/<see cref="DisableRapidTurn"/>).
		/// </summary>
		private bool disableRapidTurn
		{
			get { return objectsThatDisabledRapidTurn.Count > 0; }
		}

		/// <summary>
		/// Fired on jump.
		/// </summary>
		public event Action jumpStarted;

		/// <summary>
		/// Fired when the character starts falling.
		/// </summary>
		public event Action<float> fallStarted;

		/// <summary>
		/// The input implementation
		/// </summary>
		private IThirdPersonInput characterInput;

		/// <summary>
		/// The controller controllerAdapter implementation
		/// </summary>
		private ControllerAdapter controllerAdapter;

		private ThirdPersonGroundMovementState preTurnMovementState;
		private ThirdPersonGroundMovementState movementState = ThirdPersonGroundMovementState.Walking;
		private ThirdPersonAerialMovementState aerialState = ThirdPersonAerialMovementState.Grounded;

		private SlidingAverage averageForwardVelocity,
		                       actionAverageForwardInput,
		                       strafeAverageForwardInput,
		                       strafeAverageLateralInput;

		private float turnaroundMovementTime,
		              lastIdleTime;
		private bool jumpQueued;
		private Animator animator;
		private Vector3 fallDirection;
		private Transform transform;
		private GameObject gameObject;
		private ThirdPersonBrain thirdPersonBrain;
		private SizedQueue<Vector2> previousInputs;
		private Camera mainCamera;
		
		// on start strafe control initial look
		private bool isInitialStrafeLook;
		private float initialStrafeLookCount;
		private Quaternion rotationOnStrafeStart;

		
		/// <summary>
		/// Gets whether to track height above the ground.
		/// </summary>
		private bool trackGroundHeight;

		/// <summary>
		/// List of objects that disabled rapid turn. To allow multiple objects to disable it temporarily.
		/// </summary>
		private readonly List<object> objectsThatDisabledRapidTurn = new List<object>();

		/// <summary>
		/// Gets the current <see cref="TurnAroundBehaviour"/>.
		/// </summary>
		public TurnAroundBehaviour currentTurnAroundBehaviour
		{
			get { return thirdPersonBrain.turnAround; }
		}
	
		/// <summary>
		/// Gets the vertical speed.
		/// </summary>
		/// <value>Range = -1 (falling) to 1 (jumping).</value>
		/// <remarks>Returns <see cref="ControllerAdapter"/>'s <see cref="ControllerAdapter.normalizedVerticalSpeed"/>.</remarks>
		public float normalizedVerticalSpeed
		{
			get { return controllerAdapter.normalizedVerticalSpeed; }
		}
		
		/// <summary>
		/// Gets whether the character is in a sprint state.
		/// </summary>
		/// <value>True if in a sprint state; false otherwise.</value>
		public bool sprint { get; private set; }
		
		/// <summary>
		/// Gets the current <see cref="ThirdPersonGroundMovementState"/>.
		/// </summary>
		public ThirdPersonGroundMovementState currentGroundMovementState
		{
			get { return movementState; }
		}
		
		/// <summary>
		/// Gets the current <see cref="ThirdPersonAerialMovementState"/>.
		/// </summary>
		public ThirdPersonAerialMovementState currentAerialMovementState
		{
			get { return aerialState; }
		}

		/// <summary>
		/// Called on the exit of the root motion jump animation.
		/// </summary>
		public void OnJumpAnimationComplete()
		{
			if (controllerAdapter.IsPredictedFallShort())
			{
				OnLanding();
			}
		}

		/// <summary>
		/// Whether the character it grounded
		/// </summary>
		/// <value>True if <see cref="aerialState"/> is grounded</value>
		private bool IsGrounded
		{
			get { return aerialState == ThirdPersonAerialMovementState.Grounded; }
		}

		/// <summary>
		/// Moves the character based on movement and animator state.
		/// </summary>
		/// <remarks>Called by the Animator</remarks>
		public void OnAnimatorMove()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				controllerAdapter.Move(thirdPersonBrain.turnAround.GetMovement(), Time.deltaTime);
				return;
			}

			if (thirdPersonBrain.isRootMotionState)
			{
				Vector3 groundMovementVector = animator.deltaPosition * configuration.scaleRootMovement;
				groundMovementVector.y = 0.0f;
				
				controllerAdapter.Move(groundMovementVector, Time.deltaTime);
				
				//Update the average movement speed
				var direction = movementMode == ThirdPersonMotorMovementMode.Action
					                ? transform.forward
					                : CalculateLocalInputDirection();              
				float movementVelocity = groundMovementVector.GetMagnitudeOnAxis(direction)/Time.deltaTime;
				if (movementVelocity > 0)
				{
					averageForwardVelocity.Add(movementVelocity, HandleNegative.Absolute);
				}

				if (!characterInput.hasMovementInput)
				{
					lastIdleTime = Time.time;
				}
			}
			else //aerial
			{
				if (normalizedVerticalSpeed <= 0 || aerialState != ThirdPersonAerialMovementState.Grounded)
				{
					UpdateFallForwardSpeed();
				}

				var movementDirection = movementMode == ThirdPersonMotorMovementMode.Action ? transform.forward :
					CalculateLocalInputDirection() ;
				fallDirection = Vector3.Lerp(fallDirection, movementDirection, configuration.fallDirectionChange);
				controllerAdapter.Move(cachedForwardVelocity * Time.deltaTime * fallDirection, Time.deltaTime);
			}
		}

		public void Init(ThirdPersonBrain brain)
		{
			mainCamera = Camera.main;
			gameObject = brain.gameObject;
			transform = brain.transform;
			thirdPersonBrain = brain;
			characterInput = brain.thirdPersonInput;
			controllerAdapter = brain.controllerAdapter;
			animator = gameObject.GetComponent<Animator>();
			averageForwardVelocity = new SlidingAverage(configuration.jumpGroundVelocityWindowSize);
			actionAverageForwardInput = new SlidingAverage(configuration.forwardInputWindowSize);
			strafeAverageForwardInput = new SlidingAverage(configuration.strafeInputWindowSize);
			strafeAverageLateralInput = new SlidingAverage(configuration.strafeInputWindowSize);
			previousInputs = new SizedQueue<Vector2>(configuration.bufferSizeInput);
			movementMode = ThirdPersonMotorMovementMode.Action;

			EndStrafe();
		}

		/// <summary>
		/// Subscribe to physics, camera and input events
		/// </summary>
		public void Subscribe()
		{
			controllerAdapter.landed += OnLanding;
			controllerAdapter.startedFalling += OnStartedFalling;
			
			//Turnaround subscription for runtime support
			foreach (TurnAroundBehaviour turnaroundBehaviour in thirdPersonBrain.turnAroundOptions)
			{
				turnaroundBehaviour.turnaroundComplete += TurnaroundComplete;
			}
		}

		public void OnSprintStarted()
		{
			sprint = !sprint;
		}
		
		public void OnSprintEnded()
		{
			sprint = false;
		}

		/// <summary>
		/// Unsubscribe from events
		/// </summary>
		public void Unsubscribe()
		{
			//Physics subscriptions
			if (controllerAdapter != null)
			{
				controllerAdapter.landed -= OnLanding;
				controllerAdapter.startedFalling -= OnStartedFalling;
			}

			//Turnaround un-subscription for runtime support
			foreach (TurnAroundBehaviour turnaroundBehaviour in thirdPersonBrain.turnAroundOptions)
			{
				turnaroundBehaviour.turnaroundComplete -= TurnaroundComplete;
			}
		}

		/// <summary>
		/// Performs movement logic
		/// </summary>
		public void Update()
		{
			if (configuration.autoToggleSprintOnNoInput && sprint && !characterInput.hasMovementInput)
			{
				OnSprintEnded();
			}
			
			if (movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				CalculateForwardMovement();
			}
			else
			{
				switch (movementMode)
				{
					case ThirdPersonMotorMovementMode.Action:
						CalculateForwardMovement();
						break;
					case ThirdPersonMotorMovementMode.Strafe:
						CalculateStrafeMovement();
						break;
				}
			}
			
			previousInputs.Add(characterInput.moveInput);
			
			if (jumpQueued)
			{
				jumpQueued = TryJump();
			}
			if (trackGroundHeight)
			{
				UpdateTrackGroundHeight();
			}
		}

		public void SetLookDirection()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				return;
			}
			switch (movementMode)
			{
				case ThirdPersonMotorMovementMode.Action:
					SetActionLookDirection();
					break;
				case ThirdPersonMotorMovementMode.Strafe:
					SetStrafeLookDirection();
					break;
			}
		}

		/// <summary>
		/// Enable rapid turn. Usually used after it has been temporarily disabled.
		/// </summary>
		/// <param name="disabledByObject">The object that disabled it previously via DisableRapidTurn.</param>
		public void EnableRapidTurn(object disabledByObject)
		{
			if (objectsThatDisabledRapidTurn.Contains(disabledByObject))
			{
				previousInputs.Clear();
				objectsThatDisabledRapidTurn.Remove(disabledByObject);
			}
		}

		/// <summary>
		/// Disable rapid turn. Usually used to disable it temporarily.
		/// </summary>
		/// <param name="disabledByObject">The object that called this method. Use the same object when calling
		/// EnableRapidTurn. This helps identify various objects that temporarily disables rapid turn.</param>
		public void DisableRapidTurn(object disabledByObject)
		{
			if (!objectsThatDisabledRapidTurn.Contains(disabledByObject))
			{
				objectsThatDisabledRapidTurn.Add(disabledByObject);
			}
		}

		/// <summary>
		/// Track height above ground when the physics character is in the air, but the animation has not yet changed to
		/// the fall animation.
		/// </summary>
		private void UpdateTrackGroundHeight()
		{
			if (aerialState == ThirdPersonAerialMovementState.Grounded && !controllerAdapter.isGrounded)
			{
				if (Time.frameCount % k_TrackGroundFrameIntervals == 0)
				{
					float distance;
					if (!controllerAdapter.IsPredictedFallShort(out distance))
					{
						OnStartedFalling(distance);
					}
				}
			}
			else
			{
				trackGroundHeight = false;
			}
		}

		/// <summary>
		/// Sets the aerial state to <see cref="ThirdPersonAerialMovementState.Grounded"/> and clears
		/// <see cref="averageForwardVelocity"/> if no input.
		/// </summary>
		private void OnLanding()
		{
			aerialState = ThirdPersonAerialMovementState.Grounded;

			if (!characterInput.hasMovementInput)
			{
				averageForwardVelocity.Clear();
			}
		}

		/// <summary>
		/// Sets the aerial state to <see cref="ThirdPersonAerialMovementState.Falling"/> and fires the
		/// <see cref="fallStarted"/> event.
		/// </summary>
		/// <remarks>This subscribes to <see cref="ControllerAdapter.startedFalling"/></remarks>
		private void OnStartedFalling(float predictedFallDistance)
		{
			// check if far enough from ground to enter fall state
			if (controllerAdapter.IsPredictedFallShort())
			{
				trackGroundHeight = true;
				return;
			}
			trackGroundHeight = false;
			
			if (aerialState == ThirdPersonAerialMovementState.Grounded)
			{
				cachedForwardVelocity = averageForwardVelocity.average;
			}
			
			aerialState = ThirdPersonAerialMovementState.Falling;
			
			if (fallStarted != null)
			{
				fallStarted(predictedFallDistance);
			}
		}

		/// <summary>
		/// Queues a jump.
		/// </summary>
		public void OnJumpPressed()
		{
			jumpQueued = true;
		}

		/// <summary>
		/// Changes movement mode to <see cref="ThirdPersonMotorMovementMode.Strafe"/>
		/// </summary>
		public void StartStrafe()
		{
			if (movementMode == ThirdPersonMotorMovementMode.Strafe)
			{
				return;
			}
			
			movementMode = ThirdPersonMotorMovementMode.Strafe;
			isInitialStrafeLook = true;
			initialStrafeLookCount = configuration.initialStrafeLookDuration;
			rotationOnStrafeStart = transform.rotation;
		}
		
		/// <summary>
		/// Changes movement mode to <see cref="ThirdPersonMotorMovementMode.Action"/>
		/// </summary>
		public void EndStrafe()
		{
			movementMode = ThirdPersonMotorMovementMode.Action;
		}

		private void SetStrafeLookDirection()
		{
			Quaternion targetRotation = CalculateTargetRotation(Vector3.forward);
			targetYRotation = targetRotation.eulerAngles.y;
			Quaternion newRotation;

			if (isInitialStrafeLook)
			{
				newRotation = Quaternion.Lerp(rotationOnStrafeStart, targetRotation, 
					1.0f - initialStrafeLookCount / configuration.initialStrafeLookDuration);
				initialStrafeLookCount -= Time.deltaTime;
				if (initialStrafeLookCount <= 0.0f)
				{
					isInitialStrafeLook = false;
				}
			}
			else
			{
				newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
								configuration.turningYSpeed * configuration.strafeTurningSpeedScale * Time.deltaTime);
			}
			
			SetTurningSpeed(transform.rotation, newRotation);
			transform.rotation = newRotation;
		}

		private void SetActionLookDirection()
		{
			if (!characterInput.hasMovementInput)
			{
				normalizedTurningSpeed = 0;
				targetYRotation = transform.eulerAngles.y;
				return;
			}

			Quaternion targetRotation = CalculateTargetRotation(new Vector3(characterInput.moveInput.x, 0, characterInput.moveInput.y));
			targetYRotation = targetRotation.eulerAngles.y;

			if (IsGrounded && CheckForAndHandleRapidTurn(targetRotation))
			{
				return;
			}

			float turnSpeed = IsGrounded
				? configuration.turningYSpeed
				: configuration.jumpTurningYSpeed;

			Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

			SetTurningSpeed(transform.rotation, newRotation);

			transform.rotation = newRotation;
		}

		private void CalculateForwardMovement()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround && turnaroundMovementTime < configuration.ignoreInputTimeRapidTurn)
			{
				turnaroundMovementTime += Time.deltaTime;
				return; 
			}
			
			normalizedLateralSpeed = 0;

			var inputVector = characterInput.moveInput;
			if (inputVector.magnitude > 1)
			{
				inputVector.Normalize();
			}
			actionAverageForwardInput.Add(inputVector.magnitude + (sprint && characterInput.hasMovementInput
											  ? configuration.sprintNormalizedForwardSpeedIncrease : 0));
			
			normalizedForwardSpeed = actionAverageForwardInput.average;
		}

		private void CalculateStrafeMovement()
		{
			strafeAverageForwardInput.Add(characterInput.moveInput.y);
			float averageForwardInput = strafeAverageForwardInput.average;
			strafeAverageLateralInput.Add(characterInput.moveInput.x);
			float averageLateralInput = strafeAverageLateralInput.average;
			
			normalizedForwardSpeed =
				Mathf.Clamp((Mathf.Approximately(averageForwardInput, 0f) ? 0f : averageForwardInput),
							-configuration.normalizedBackwardStrafeSpeed, configuration.normalizedForwardStrafeSpeed);
			normalizedLateralSpeed = Mathf.Approximately(averageLateralInput, 0f)
				? 0f : averageLateralInput * configuration.normalizedLateralStrafeSpeed;
		}

		private Vector3 CalculateLocalInputDirection()
		{
			var localMovementDirection = new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);
			return Quaternion.AngleAxis(mainCamera.transform.eulerAngles.y, Vector3.up) * 
			       localMovementDirection.normalized;
		}

		private Quaternion CalculateTargetRotation(Vector3 localDirection)
		{
			Vector3 flatForward = CalculateCharacterBearing();
			
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}
		
		private Vector3 CalculateCharacterBearing()
		{
			Vector3 bearing = mainCamera.transform.forward;
			bearing.y = 0f;
			bearing.Normalize();

			return bearing;
		}

		/// <summary>
		/// Sets <see cref="normalizedForwardSpeed"/> so that a turn will approach the desired rotation.
		/// </summary>
		/// <param name="currentRotation">Current rotation.</param>
		/// <param name="newRotation">Desired rotation.</param>
		private void SetTurningSpeed(Quaternion currentRotation, Quaternion newRotation)
		{
			float currentY = currentRotation.eulerAngles.y;
			float newY = newRotation.eulerAngles.y;
			float difference = (newY - currentY).Wrap180() / Time.deltaTime;

			normalizedTurningSpeed = Mathf.Lerp(normalizedTurningSpeed,Mathf.Clamp(
													difference / configuration.turningYSpeed *
													configuration.turningSpeedScaleVisual, -1, 1),
													Time.deltaTime * configuration.normalizedTurningSpeedLerpSpeedFactor);
		}

		/// <remarks>Subscribes to the <see cref="currentTurnAroundBehaviour"/>'s
		/// <see cref="TurnAroundBehaviour.turnaroundComplete"/> </remarks>
		private void TurnaroundComplete()
		{
			movementState = preTurnMovementState;
		}

		private bool CheckForAndHandleRapidTurn(Quaternion target)
		{
			if (thirdPersonBrain.turnAround == null || disableRapidTurn)
			{
				return false;
			}
			
			float angle;
			if (ShouldTurnAround(out angle, target))
			{
				StartTurnAround(angle);
				return true;
			}
			return false;
		}

		private void StartTurnAround(float angle)
		{
			turnaroundMovementTime = 0f;
			cachedForwardVelocity = averageForwardVelocity.average;
			preTurnMovementState = movementState;
			movementState = ThirdPersonGroundMovementState.TurningAround;
			jumpQueued = false;
			thirdPersonBrain.turnAround.TurnAround(angle);
			thirdPersonBrain.turnAround.turnaroundComplete += OnTurnAroundComplete;
		}

		private void OnTurnAroundComplete()
		{
			thirdPersonBrain.turnAround.turnaroundComplete -= OnTurnAroundComplete;

			if (!characterInput.hasMovementInput)
			{
				return;
			}
			Quaternion target = CalculateTargetRotation(
				new Vector3(characterInput.moveInput.x, 0, characterInput.moveInput.y));
			var angle = (target.eulerAngles.y - transform.eulerAngles.y).Wrap180();
			if (Mathf.Abs(angle) > configuration.stationaryAngleRapidTurn)
			{
				StartTurnAround(angle);
			}
		}

		/// <summary>
		/// Decides whether a rapid turn should be initiated.
		/// </summary>
		/// <param name="angle">The angle of the rapid turn. 0 if no rapid turn was detected.</param>
		/// <param name="target">Target character direction.</param>
		/// <returns>True is a rapid turn has been detected.</returns>
		private bool ShouldTurnAround(out float angle, Quaternion target)
		{
			if (normalizedForwardSpeed < configuration.standingTurnaroundSpeedThreshold)
			{
				previousInputs.Clear();
				angle = (target.eulerAngles.y - transform.eulerAngles.y).Wrap180();
				return Mathf.Abs(angle) > configuration.stationaryAngleRapidTurn;
			}

			foreach (Vector2 previousInputsValue in previousInputs.values)
			{
				angle = -Vector2.SignedAngle(previousInputsValue, characterInput.moveInput);
				float deltaMagnitude = Mathf.Abs(previousInputsValue.magnitude - characterInput.moveInput.magnitude);
				if (Mathf.Abs(angle) > configuration.inputAngleRapidTurn && deltaMagnitude < 0.25f)
				{
					previousInputs.Clear();
					return true;
				}
			}
			angle = 0;
			return false;
		}
		
		/// <summary>
		/// Attempts a jump. If successful fires the <see cref="jumpStarted"/> event and
		/// sets <see cref="aerialState"/> to <see cref="ThirdPersonAerialMovementState.Jumping"/>.
		/// </summary>
		/// <returns>True if a jump should be re-attempted</returns>
		private bool TryJump()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround || 
			    thirdPersonBrain.animatorState == ThirdPersonBrain.AnimatorState.Landing)
			{
				return true;
			}
			if (!IsGrounded || controllerAdapter.startedSlide || !thirdPersonBrain.isRootMotionState)
			{
				return false;
			}
			
			aerialState = ThirdPersonAerialMovementState.Jumping;
			
			// check for a standing forward jump.
			if (characterInput.moveInput.magnitude > configuration.standingJumpMinInputThreshold && 
				lastIdleTime + configuration.standingJumpMoveThresholdTime >= Time.time  &&
				animator.deltaPosition.GetMagnitudeOnAxis(transform.forward) <= 
				configuration.standingJumpMaxMovementThreshold * Time.deltaTime)
			{
				cachedForwardVelocity = configuration.standingJumpSpeed;
				normalizedForwardSpeed = 1;
				thirdPersonBrain.UpdateForwardSpeed(normalizedForwardSpeed, 1);
				fallDirection = transform.forward;
			}
			else
			{
				fallDirection = movementMode == ThirdPersonMotorMovementMode.Action ? 
					                transform.forward : CalculateLocalInputDirection();
				cachedForwardVelocity = averageForwardVelocity.average;
			}
			
			controllerAdapter.SetJumpVelocity(
				configuration.jumpHeightAsFactorOfForwardSpeed.Evaluate(normalizedForwardSpeed));
			
			if (jumpStarted != null)
			{
				jumpStarted();
			}
			return false;
		}
		
		private void UpdateFallForwardSpeed()
		{
			float maxFallForward = configuration.fallingForwardSpeed;
			float target = maxFallForward * Mathf.Clamp01(characterInput.moveInput.magnitude);
			float time = cachedForwardVelocity > target
				             ? configuration.fallSpeedDeceleration
				             : configuration.fallSpeedAcceleration;
			cachedForwardVelocity = Mathf.Lerp(cachedForwardVelocity, target, time);
			normalizedForwardSpeed = cachedForwardVelocity / maxFallForward;
		}
	}
	
	/// <summary>
	/// Enum used to describe the third person aerial movement state.
	/// </summary>
	public enum ThirdPersonAerialMovementState
	{
		Grounded,
		Jumping,
		Falling
	}
	
	/// <summary>
	/// Enum used to describe the third person ground movement state.
	/// </summary>
	public enum ThirdPersonGroundMovementState
	{
		Walking,
		Running,
		TurningAround
	}
	
	/// <summary>
	/// Enum used to describe the third person movement mode.
	/// </summary>
	public enum ThirdPersonMotorMovementMode
	{
		Action,
		Strafe
	}
}
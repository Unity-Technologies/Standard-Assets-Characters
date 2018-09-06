using System;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// IThirdPersonMotor implementation that uses, primarily, root motion from the animator to move the character.
	/// </summary>
	[Serializable]
	public class RootMotionThirdPersonMotor : IThirdPersonMotor
	{
		/// <summary>
		/// Track distance above the ground at these frame intervals (to prevent checking every frame)
		/// </summary>
		private const int k_TrackGroundFrameIntervals = 5;
		
		/// <summary>
		/// Various configuration settings more movement.
		/// </summary>
		[SerializeField, Tooltip("Reference to the configuration with all the movement settings")]
		protected ThirdPersonRootMotionConfiguration configuration;
		
		[SerializeField, Tooltip("Input response to trigger sprint")]
		protected InputResponse sprintInput;
		
		[SerializeField, Tooltip("Settings to customize character rotation")]
		protected CharacterRotator rotator;

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
			get { return characterPhysics.fallTime; }
		}

		/// <summary>
		/// Gets the current target y rotation.
		/// </summary>
		/// <value>The current y rotation, in degrees, used by the <see cref="rotator"/>.</value>
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

		/// <inheritdoc />
		public event Action jumpStarted;

		/// <inheritdoc />
		/// <remarks>Subscribes to <see cref="ICharacterPhysics.landed"/>.</remarks>
		public event Action landed;

		/// <inheritdoc />
		public event Action<float> fallStarted;

		/// <inheritdoc />
		public event Action<float> rapidlyTurned;

		/// <summary>
		/// The input implementation
		/// </summary>
		protected ICharacterInput characterInput;

		/// <summary>
		/// The physic implementation
		/// </summary>
		protected ICharacterPhysics characterPhysics;

		protected ThirdPersonAnimationController animationController;

		protected ThirdPersonGroundMovementState preTurnMovementState;
		protected ThirdPersonGroundMovementState movementState = ThirdPersonGroundMovementState.Walking;

		protected ThirdPersonAerialMovementState aerialState = ThirdPersonAerialMovementState.Grounded;

		/// <summary>
		/// Sliding average of root motion velocity.
		/// </summary>
		private SlidingAverage averageForwardVelocity;

		private SlidingAverage actionAverageForwardInput, strafeAverageForwardInput, strafeAverageLateralInput;

		private float turnaroundMovementTime;
		private bool jumpQueued;
		private Animator animator;
		private Vector3 fallDirection;
		private Transform transform;
		private GameObject gameObject;
		private ThirdPersonBrain thirdPersonBrain;
		private SizedQueue<Vector2> previousInputs;
		
		/// <summary>
		/// Gets whether to track height above the ground.
		/// </summary>
		private bool trackGroundHeight;

		/// <inheritdoc />
		public TurnaroundBehaviour currentTurnaroundBehaviour
		{
			get { return thirdPersonBrain.turnaround; }
		}
		/// <inheritdoc />
		public float normalizedVerticalSpeed
		{
			get { return characterPhysics.normalizedVerticalSpeed; }
		}
		
		/// <summary>
		/// Gets whether the character is in a sprint state.
		/// </summary>
		/// <value>True if in a sprint state; false otherwise.</value>
		public bool sprint { get; private set; }
		
		/// <inheritdoc />
		public ThirdPersonGroundMovementState currentGroundMovementState
		{
			get { return movementState; }
		}
		
		/// <inheritdoc />
		public ThirdPersonAerialMovementState currentAerialMovementState
		{
			get { return aerialState; }
		}

		/// <summary>
		/// Called on the exit of the root motion jump animation.
		/// </summary>
		/// <remarks>Should only be called by a root motion jump StateMachineBehaviour</remarks>
		public void OnJumpAnimationComplete()
		{
			var baseCharacterPhysics = characterPhysics as BaseCharacterPhysics;
			if (baseCharacterPhysics == null)
			{
				return;
			}

			var distance = baseCharacterPhysics.GetPredictedFallDistance();
			if (distance <= configuration.maxFallDistanceToLand)
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
				characterPhysics.Move(thirdPersonBrain.turnaround.GetMovement(), Time.deltaTime);
				return;
			}

			if (animationController.isRootMotionState)
			{
				Vector3 groundMovementVector = animator.deltaPosition * configuration.scaleRootMovement;
				groundMovementVector.y = 0;
				characterPhysics.Move(groundMovementVector, Time.deltaTime);
				
				//Update the average movement speed
				float movementVelocity = groundMovementVector.
										 GetMagnitudeOnAxis(transform.forward)/Time.deltaTime;
				if (movementVelocity > 0)
				{
					averageForwardVelocity.Add(movementVelocity, HandleNegative.Absolute);
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
				characterPhysics.Move(cachedForwardVelocity * Time.deltaTime * fallDirection, Time.deltaTime);
			}
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

		public void Init(ThirdPersonBrain brain)
		{
			gameObject = brain.gameObject;
			transform = brain.transform;
			thirdPersonBrain = brain;
			characterInput = brain.inputForCharacter;
			characterPhysics = brain.physicsForCharacter;
			animator = gameObject.GetComponent<Animator>();
			animationController = brain.animationControl;
			averageForwardVelocity = new SlidingAverage(configuration.jumpGroundVelocityWindowSize);
			actionAverageForwardInput = new SlidingAverage(configuration.forwardInputWindowSize);
			strafeAverageForwardInput = new SlidingAverage(configuration.strafeInputWindowSize);
			strafeAverageLateralInput = new SlidingAverage(configuration.strafeInputWindowSize);
			previousInputs = new SizedQueue<Vector2>(configuration.bufferSizeInput);
			movementMode = ThirdPersonMotorMovementMode.Action;

			if (sprintInput != null)
			{
				sprintInput.Init();
			}

			OnStrafeEnded();
		}

		/// <summary>
		/// Subscribe to physics, camera and input events
		/// </summary>
		public void Subscribe()
		{
			characterPhysics.landed += OnLanding;
			characterPhysics.startedFalling += OnStartedFalling;
			characterInput.jumpPressed += OnJumpPressed;
			
			if (thirdPersonBrain.thirdPersonCameraAnimationManager != null)
			{
				thirdPersonBrain.thirdPersonCameraAnimationManager.forwardLockedModeStarted += OnStrafeStarted;
				thirdPersonBrain.thirdPersonCameraAnimationManager.forwardUnlockedModeStarted += OnStrafeEnded;
			}
			
			if (sprintInput != null)
			{
				sprintInput.started += OnSprintStarted;
				sprintInput.ended += OnSprintEnded;
			}
			
			//Turnaround subscription for runtime support
			foreach (TurnaroundBehaviour turnaroundBehaviour in thirdPersonBrain.turnaroundOptions)
			{
				turnaroundBehaviour.turnaroundComplete += TurnaroundComplete;
			}
		}

		private void OnSprintStarted()
		{
			sprint = !sprint;
		}
		
		private void OnSprintEnded()
		{
			sprint = false;
		}

		/// <summary>
		/// Unsubscribe from events
		/// </summary>
		public void Unsubscribe()
		{
			//Physics subscriptions
			if (characterPhysics != null)
			{
				characterPhysics.landed -= OnLanding;
				characterPhysics.startedFalling -= OnStartedFalling;
			}

			//Input subscriptions
			if (characterInput != null)
			{
				characterInput.jumpPressed -= OnJumpPressed;
			}

			if (thirdPersonBrain.thirdPersonCameraAnimationManager != null)
			{
				thirdPersonBrain.thirdPersonCameraAnimationManager.forwardLockedModeStarted -= OnStrafeStarted;
				thirdPersonBrain.thirdPersonCameraAnimationManager.forwardUnlockedModeStarted -= OnStrafeEnded;
			}
			
			if (sprintInput != null)
			{
				sprintInput.started -= OnSprintStarted;
				sprintInput.ended -= OnSprintEnded;
			}

			//Turnaround un-subscription for runtime support
			foreach (TurnaroundBehaviour turnaroundBehaviour in thirdPersonBrain.turnaroundOptions)
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
				sprintInput.ManualInputEnded();
			}
			
			HandleMovement();
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

		/// <summary>
		/// Track height above ground when the physics character is in the air, but the animation has not yet changed to
		/// the fall animation.
		/// </summary>
		private void UpdateTrackGroundHeight()
		{
			if (aerialState == ThirdPersonAerialMovementState.Grounded && !characterPhysics.isGrounded)
			{
				if (Time.frameCount % k_TrackGroundFrameIntervals == 0)
				{
					var baseCharacterPhysics = characterPhysics as BaseCharacterPhysics;
					if (baseCharacterPhysics != null)
					{
						float distance = baseCharacterPhysics.GetPredictedFallDistance();
						if (distance > configuration.maxFallDistanceToLand)
						{
							OnStartedFalling(distance);
						}
					}
					else
					{
						trackGroundHeight = false;
					}
				}
			}
			else
			{
				trackGroundHeight = false;
			}
		}

		/// <summary>
		/// Sets the aerial state to <see cref="ThirdPersonAerialMovementState.Grounded"/> and fires
		/// the <see cref="landed"/> event.
		/// </summary>
		/// <remarks>This subscribes to <see cref="ICharacterPhysics.landed"/></remarks>
		protected virtual void OnLanding()
		{
			aerialState = ThirdPersonAerialMovementState.Grounded;

			if (!characterInput.hasMovementInput)
			{
				averageForwardVelocity.Clear();
			}

			if (landed != null)
			{
				landed();
			}
		}

		/// <summary>
		/// Sets the aerial state to <see cref="ThirdPersonAerialMovementState.Falling"/> and fires the
		/// <see cref="fallStarted"/> event.
		/// </summary>
		/// <remarks>This subscribes to <see cref="ICharacterPhysics.startedFalling"/></remarks>
		protected virtual void OnStartedFalling(float predictedFallDistance)
		{
			// check if far enough from ground to enter fall state
			if (predictedFallDistance < configuration.maxFallDistanceToLand)
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
		/// <remarks>This subscribes to <see cref="ICharacterInput.jumpPressed"/></remarks>
		protected virtual void OnJumpPressed()
		{
			jumpQueued = true;
		}

		/// <summary>
		/// Changes movement mode to <see cref="ThirdPersonMotorMovementMode.Strafe"/>
		/// </summary>
		/// <remarks>This subscribes <see cref="ThirdPersonCameraAnimationManager.forwardLockedModeStarted"/></remarks>
		protected virtual void OnStrafeStarted()
		{
			if (movementMode == ThirdPersonMotorMovementMode.Strafe)
			{
				return;
			}
			
			movementMode = ThirdPersonMotorMovementMode.Strafe;
		}

		/// <summary>
		/// Changes movement mode to <see cref="ThirdPersonMotorMovementMode.Action"/>
		/// </summary>
		/// <remarks>This subscribes <see cref="ThirdPersonCameraAnimationManager.forwardUnlockedModeStarted"/></remarks>
		protected virtual void OnStrafeEnded()
		{
			movementMode = ThirdPersonMotorMovementMode.Action;
		}

		/// <summary>
		/// Handles movement based on <see cref="movementMode"/> and <see cref="movementMode"/>
		/// </summary>
		protected virtual void HandleMovement()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				CalculateForwardMovement();
				return;
			}

			switch (movementMode)
			{
				case ThirdPersonMotorMovementMode.Action:
					ActionMovement();
					break;
				case ThirdPersonMotorMovementMode.Strafe:
					StrafeMovement();
					break;
			}
		}

		protected virtual void ActionMovement()
		{
			SetLookDirection();
			CalculateForwardMovement();
		}

		protected virtual void StrafeMovement()
		{
			SetStrafeLookDirection();
			CalculateStrafeMovement();
		}

		protected virtual void SetStrafeLookDirection()
		{
			Quaternion targetRotation = CalculateTargetRotation(Vector3.forward);

			targetYRotation = targetRotation.eulerAngles.y;

			Quaternion newRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation,
										 configuration.turningYSpeed * Time.deltaTime);

			SetTurningSpeed(transform.rotation, newRotation);

			transform.rotation = newRotation;
		}

		protected virtual void SetLookDirection()
		{
			if (!characterInput.hasMovementInput)
			{
				normalizedTurningSpeed = 0;
				targetYRotation = transform.eulerAngles.y;
				return;
			}

			Quaternion targetRotation = CalculateTargetRotation();
			targetYRotation = targetRotation.eulerAngles.y;

			if (IsGrounded && CheckForAndHandleRapidTurn(targetRotation))
			{
				return;
			}

			float turnSpeed = IsGrounded
				? configuration.turningYSpeed
				: configuration.jumpTurningYSpeed;

			rotator.Tick(targetYRotation);
			Quaternion newRotation = rotator.GetNewRotation(transform, targetRotation, turnSpeed);

			SetTurningSpeed(transform.rotation, newRotation);

			transform.rotation = newRotation;
		}

		protected virtual void CalculateForwardMovement()
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

		protected virtual void CalculateStrafeMovement()
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

		protected virtual Quaternion CalculateTargetRotation()
		{
			return CalculateTargetRotation(new Vector3(characterInput.moveInput.x, 0, characterInput.moveInput.y));
		}

		protected virtual Vector3 CalculateLocalInputDirection()
		{
			var localMovementDirection = new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);
			return Quaternion.AngleAxis(thirdPersonBrain.bearingOfCharacter.cameraMain.eulerAngles.y, Vector3.up) * 
			       localMovementDirection.normalized;
		}

		protected virtual Quaternion CalculateTargetRotation(Vector3 localDirection)
		{
			Vector3 flatForward = thirdPersonBrain.bearingOfCharacter.CalculateCharacterBearing();
			
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		/// <summary>
		/// Sets <see cref="normalizedForwardSpeed"/> so that a turn will approach the desired rotation.
		/// </summary>
		/// <param name="currentRotation">Current rotation.</param>
		/// <param name="newRotation">Desired rotation.</param>
		protected virtual void SetTurningSpeed(Quaternion currentRotation, Quaternion newRotation)
		{
			float currentY = currentRotation.eulerAngles.y;
			float newY = newRotation.eulerAngles.y;
			float difference = (MathUtilities.Wrap180(newY) - MathUtilities.Wrap180(currentY)) / Time.deltaTime;

			normalizedTurningSpeed = Mathf.Lerp(normalizedTurningSpeed,Mathf.Clamp(
													difference / configuration.turningYSpeed *
													configuration.turningSpeedScaleVisual, -1, 1),
													Time.deltaTime * configuration.normalizedTurningSpeedLerpSpeedFactor);
		}

		/// <remarks>Subscribes to the <see cref="currentTurnaroundBehaviour"/>'s
		/// <see cref="TurnaroundBehaviour.turnaroundComplete"/> </remarks>
		protected virtual void TurnaroundComplete()
		{
			movementState = preTurnMovementState;
		}

		protected virtual bool CheckForAndHandleRapidTurn(Quaternion target)
		{
			if (thirdPersonBrain.turnaround == null)
			{
				return false;
			}

			
			float angle;

			if (ShouldTurnAround(out angle, target))
			{
				turnaroundMovementTime = 0f;
				cachedForwardVelocity = averageForwardVelocity.average;
				preTurnMovementState = movementState;
				movementState = ThirdPersonGroundMovementState.TurningAround;
				thirdPersonBrain.turnaround.TurnAround(angle);
				if (rapidlyTurned != null)
				{
					rapidlyTurned(angle);
				}
				return true;
			}

			return false;
		}

		/// <summary>
		/// Decides whether a rapid turn should be initiated.
		/// </summary>
		/// <param name="angle">The angle of the rapid turn. 0 if no rapid turn was detected.</param>
		/// <param name="target">Target character direction.</param>
		/// <returns>True is a rapid turn has been detected.</returns>
		protected virtual bool ShouldTurnAround(out float angle, Quaternion target)
		{
			if (Mathf.Approximately(normalizedForwardSpeed, 0))
			{
				previousInputs.Clear();
				float currentY = transform.eulerAngles.y;
				float newY = target.eulerAngles.y;
				angle = MathUtilities.Wrap180(newY - currentY);
				return Mathf.Abs(angle) > configuration.stationaryAngleRapidTurn;
			}

			foreach (Vector2 previousInputsValue in previousInputs.values)
			{
				angle = Vector2.SignedAngle(previousInputsValue, characterInput.moveInput);
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
				animationController.state == AnimationState.Landing)
			{
				return true;
			}
			if (!IsGrounded || characterPhysics.startedSlide || !animationController.isRootMotionState)
			{
				return false;
			}
			
			aerialState = ThirdPersonAerialMovementState.Jumping;
			
			if (Mathf.Abs(normalizedLateralSpeed) <= normalizedForwardSpeed && normalizedForwardSpeed >=0)
			{
				// check for a standing forward jump.
				if (characterInput.moveInput.magnitude > configuration.standingJumpMinInputThreshold && 
					animator.deltaPosition.GetMagnitudeOnAxis(transform.forward) <= 
					configuration.standingJumpMaxMovementThreshold * Time.deltaTime)
				{
					cachedForwardVelocity = configuration.standingJumpSpeed;
					normalizedForwardSpeed = 1;
					animationController.UpdateForwardSpeed(normalizedForwardSpeed, 1);
				}
				else
				{
					cachedForwardVelocity = averageForwardVelocity.average;
				}
				
				characterPhysics.SetJumpVelocity(
					configuration.jumpHeightAsFactorOfForwardSpeed.Evaluate(normalizedForwardSpeed));
				
				fallDirection = transform.forward;
			}
			
			if (jumpStarted != null)
			{
				jumpStarted();
			}
			return false;
		}
	}
}
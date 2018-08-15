using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class RootMotionThirdPersonMotor : IThirdPersonMotor
	{
		
		//Strafe Camera
		[SerializeField]
		protected CinemachineFreeLook strafeFreeLookCamera;
		
		//Events
		public event Action startActionMode, startStrafeMode;

		//Serialized Fields
		[SerializeField]
		protected ThirdPersonRootMotionConfiguration configuration;

		[SerializeField]
		protected bool useRapidTurnForStrafeTransition = true;
		
		[SerializeField]
		protected InputResponse sprintInput;
		
		[SerializeField]
		protected CharacterRotator rotator;

		//Properties
		public float normalizedTurningSpeed { get; private set; }
		public float normalizedLateralSpeed { get; private set; }
		public float normalizedForwardSpeed { get; private set; }

		public float fallTime
		{
			get { return characterPhysics.fallTime; }
		}

		public float targetYRotation { get; private set; }
		
		public float cachedForwardMovement { get; protected set; }

		public Action jumpStarted { get; set; }
		public Action landed { get; set; }
		public Action<float> fallStarted { get; set; }
		public Action<float> rapidlyTurned { get; set; }

		//Protected fields

		/// <summary>
		/// The input implementation
		/// </summary>
		protected ICharacterInput characterInput;

		/// <summary>
		/// The physic implementation
		/// </summary>
		protected ICharacterPhysics characterPhysics;

		protected ThirdPersonAnimationController animationController;

		protected Animator animator;

		protected ThirdPersonMotorMovementMode movementMode = ThirdPersonMotorMovementMode.Action;

		protected ThirdPersonGroundMovementState preTurnMovementState;
		protected ThirdPersonGroundMovementState movementState = ThirdPersonGroundMovementState.Walking;

		protected ThirdPersonAerialMovementState aerialState = ThirdPersonAerialMovementState.Grounded;

		protected SlidingAverage averageForwardMovement;

		protected SlidingAverage actionAverageForwardInput, strafeAverageForwardInput, strafeAverageLateralInput;

		private bool isTurningIntoStrafe,
					 jumpQueued;
		protected Transform transform;
		protected GameObject gameObject;
		protected ThirdPersonBrain thirdPersonBrain;
		protected float turnaroundMovementTime;

		protected SizedQueue<Vector2> previousInputs;

		protected bool isStrafing
		{
			get { return movementMode == ThirdPersonMotorMovementMode.Strafe; }
		}

		public float normalizedVerticalSpeed
		{
			get { return characterPhysics.normalizedVerticalSpeed; }
		}

		public ThirdPersonRootMotionConfiguration thirdPersonConfiguration
		{
			get { return configuration; }
		}
		
		public bool sprint { get; private set; }

		public void OnJumpAnimationComplete()
		{
			var baseCharacterPhysics = characterPhysics as BaseCharacterPhysics;
			if (baseCharacterPhysics == null)
			{
				return;
			}

			var distance = baseCharacterPhysics.GetPredicitedFallDistance();
			if (distance <= configuration.maxFallDistanceToLand)
			{
				OnLanding();
			}
			else
			{
				aerialState = ThirdPersonAerialMovementState.Falling;
				if (fallStarted != null)
				{
					fallStarted(distance);
				}
			}
		}

		//Unity Messages
		public void OnAnimatorMove()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				characterPhysics.Move(thirdPersonBrain.turnaround.GetMovement());
				return;
			}

			if (ShouldApplyRootMotion())
			{
				Vector3 groundMovementVector = animator.deltaPosition * configuration.scaleRootMovement;
				groundMovementVector.y = 0;
				characterPhysics.Move(groundMovementVector);
			}
			else
			{
				if (aerialState == ThirdPersonAerialMovementState.Falling)
				{
					cachedForwardMovement = Mathf.Lerp(cachedForwardMovement, configuration.fallingForwardSpeed *
														Time.deltaTime * characterInput.moveInput.normalized.magnitude, 
														configuration.fallSpeedLerp);
				}
				characterPhysics.Move(cachedForwardMovement * transform.forward * configuration.scaledGroundVelocity);
			}
		}

		private bool ShouldApplyRootMotion()
		{
			return characterPhysics.isGrounded && animationController.shouldUseRootMotion &&
					!animationController.isRootMovement;
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
			averageForwardMovement = new SlidingAverage(configuration.jumpGroundVelocityWindowSize);
			actionAverageForwardInput = new SlidingAverage(configuration.forwardInputWindowSize);
			strafeAverageForwardInput = new SlidingAverage(configuration.strafeInputWindowSize);
			strafeAverageLateralInput = new SlidingAverage(configuration.strafeInputWindowSize);
			previousInputs = new SizedQueue<Vector2>(configuration.bufferSizeInput);

			if (sprintInput != null)
			{
				sprintInput.Init();
			}

			OnStrafeEnded();
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		public void Subscribe()
		{
			//Physics subscriptions
			characterPhysics.landed += OnLanding;
			characterPhysics.startedFalling += OnStartedFalling;

			//Input subscriptions
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
			sprint = true;
		}
		
		private void OnSprintEnded()
		{
			sprint = false;
		}

		/// <summary>
		/// Unsubscribe
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

		public void Update()
		{
			HandleMovement();
			previousInputs.Add(characterInput.moveInput);
			if (jumpQueued)
			{
				jumpQueued = TryJump();
			}
		}

		//Protected Methods
		/// <summary>
		/// Handles player landing
		/// </summary>
		protected virtual void OnLanding()
		{
			aerialState = ThirdPersonAerialMovementState.Grounded;

			if (!characterInput.hasMovementInput)
			{
				averageForwardMovement.Clear();
			}

			if (landed != null)
			{
				landed();
			}
		}

		/// <summary>
		/// Handles player falling
		/// </summary>
		/// <param name="predictedFallDistance"></param>
		protected virtual void OnStartedFalling(float predictedFallDistance)
		{
			if (aerialState == ThirdPersonAerialMovementState.Grounded)
			{
				cachedForwardMovement = averageForwardMovement.average;
			}

			aerialState = ThirdPersonAerialMovementState.Falling;

			if (fallStarted != null)
			{
				fallStarted(predictedFallDistance);
			}
		}

		/// <summary>
		/// Subscribes to the Jump action on input
		/// </summary>
		protected virtual void OnJumpPressed()
		{
			jumpQueued = true;
		}

		/// <summary>
		/// Method called by strafe input started
		/// </summary>
		protected virtual void OnStrafeStarted()
		{
			if (movementMode == ThirdPersonMotorMovementMode.Strafe)
			{
				return;
			}
			
			if (startStrafeMode != null)
			{
				startStrafeMode();
			}

			movementMode = ThirdPersonMotorMovementMode.Strafe;

			isTurningIntoStrafe = true;
		}

		/// <summary>
		/// Method called by strafe input ended
		/// </summary>
		protected virtual void OnStrafeEnded()
		{
			if (startActionMode != null)
			{
				startActionMode();
			}

			movementMode = ThirdPersonMotorMovementMode.Action;
		}

		/// <summary>
		/// Called by update to handle movement
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
			if (!isTurningIntoStrafe)
			{
				SetStrafeLookDirection();
			}
			else
			{
				SetStartStrafeLookDirection();
			}

			CalculateStrafeMovement();
		}

		protected virtual void SetStrafeLookDirection()
		{
			Quaternion targetRotation = CalculateTargetRotation(0, 1);

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
				return;
			}

			Quaternion targetRotation = CalculateTargetRotation();
			targetYRotation = targetRotation.eulerAngles.y;

			if (characterPhysics.isGrounded && CheckForAndHandleRapidTurn(targetRotation))
			{
				return;
			}

			float turnSpeed = characterPhysics.isGrounded
				? configuration.turningYSpeed
				: configuration.jumpTurningYSpeed;

			rotator.Tick(targetYRotation);
			Quaternion newRotation = rotator.GetNewRotation(transform, targetRotation, turnSpeed);

			SetTurningSpeed(transform.rotation, newRotation);

			transform.rotation = newRotation;
		}

		protected virtual void SetStartStrafeLookDirection()
		{
			Vector3 cameraForward = thirdPersonBrain.bearingOfCharacter.CalculateCharacterBearing();

			Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
			
			// TODO hack to fix camera spin
			transform.rotation = targetRotation;
			isTurningIntoStrafe = false;
			//
			
			targetYRotation = MathUtilities.Wrap180(targetRotation.eulerAngles.y);

			if (useRapidTurnForStrafeTransition && CheckForAndHandleRapidTurn(targetRotation))
			{
				return;
			}

			Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
															  configuration.turningYSpeed * Time.deltaTime);
			SetTurningSpeed(transform.rotation, newRotation);

			if (transform.rotation == targetRotation)
			{
				isTurningIntoStrafe = false;
			}

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

			
			
			if (characterPhysics.isGrounded && !animationController.isRootMovement)
			{
				Vector3 groundMovementVector = animator.deltaPosition * configuration.scaleRootMovement;
				groundMovementVector.y = 0;
	
				float value = groundMovementVector.GetMagnitudeOnAxis(transform.forward);
				if (value > 0)
				{
					averageForwardMovement.Add(value);
				}
			}
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
			return CalculateTargetRotation(characterInput.moveInput.x, characterInput.moveInput.y);
		}

		protected virtual Quaternion CalculateTargetRotation(float x, float y)
		{
			Vector3 flatForward = thirdPersonBrain.bearingOfCharacter.CalculateCharacterBearing();

			Vector3 localMovementDirection =
				new Vector3(x, 0f, y);
			Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, localMovementDirection);
			cameraToInputOffset.eulerAngles = new Vector3(0f, cameraToInputOffset.eulerAngles.y, 0f);

			return Quaternion.LookRotation(cameraToInputOffset * flatForward);
		}

		protected virtual void SetTurningSpeed(Quaternion currentRotation, Quaternion newRotation)
		{
			float currentY = currentRotation.eulerAngles.y;
			float newY = newRotation.eulerAngles.y;
			float difference = (MathUtilities.Wrap180(newY) - MathUtilities.Wrap180(currentY)) / Time.deltaTime;

			normalizedTurningSpeed = Mathf.Lerp(normalizedTurningSpeed,
												Mathf.Clamp(
													difference / configuration.turningYSpeed *
													configuration.turningSpeedScaleVisual, -1, 1),
												Time.deltaTime * configuration.turningLerpFactor);
		}

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
				cachedForwardMovement = averageForwardMovement.average;
				preTurnMovementState = movementState;
				movementState = ThirdPersonGroundMovementState.TurningAround;
				thirdPersonBrain.turnaround.TurnAround(angle);
				return true;
			}

			return false;
		}

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
				angle = MathUtilities.Wrap180(Vector2Utilities.Angle(previousInputsValue, characterInput.moveInput));
				var deltaMagnitude = Mathf.Abs(previousInputsValue.magnitude - characterInput.moveInput.magnitude);
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
		/// Attempts a jump
		/// </summary>
		/// <returns>True if a jump should be re-attempted</returns>
		private bool TryJump()
		{
			if (movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				return true;
			}
			if (!characterPhysics.isGrounded || characterPhysics.startedSlide || !animationController.canJump)
			{
				return false;
			}
			
			aerialState = ThirdPersonAerialMovementState.Jumping;
			
			if (Mathf.Abs(normalizedLateralSpeed) <= normalizedForwardSpeed && normalizedForwardSpeed >=0)
			{
				if (characterInput.moveInput.magnitude > configuration.standingJumpMinInputThreshold && 
				    animator.deltaPosition.magnitude <= configuration.standingJumpMaxMovementThreshold)
				{
					cachedForwardMovement = configuration.standingJumpSpeed;
				}
				else
				{
					cachedForwardMovement = averageForwardMovement.average;
				}
				characterPhysics.SetJumpVelocity(configuration.initialJumpVelocity);
			}
			
			if (jumpStarted != null)
			{
				jumpStarted();
			}
			return false;
		}
	}
}
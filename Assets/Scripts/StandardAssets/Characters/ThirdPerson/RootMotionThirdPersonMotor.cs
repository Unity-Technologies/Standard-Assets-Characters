using System;
using System.Collections.Generic;
using Attributes;
using Attributes.Types;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class RootMotionThirdPersonMotor : IThirdPersonMotor
	{
		//Events
		public event Action startActionMode, startStrafeMode;

		//Serialized Fields
		[SerializeField]
		protected ThirdPersonRootMotionConfiguration configuration;

		[SerializeField]
		protected bool useRapidTurnForStrafeTransition = true;

		[SerializeField]
		protected Transform cameraTransform;

		[SerializeField]
		protected InputResponse strafeInput;
		
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

		private bool isTurningIntoStrafe;
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
			return characterPhysics.isGrounded && animationController.shouldUseRootMotion;
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
			rotator.Init(characterInput);
			previousInputs = new SizedQueue<Vector2>(10);
			
			if (cameraTransform == null)
			{
				cameraTransform = Camera.main.transform;
			}

			if (strafeInput != null)
			{
				strafeInput.Init();
			}

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

			if (strafeInput != null)
			{
				strafeInput.started += OnStrafeStarted;
				strafeInput.ended += OnStrafeEnded;
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

		[SerializeField]
		protected float sprintAnimatorSpeed = 1.1f;

		public bool sprint { get; private set; }

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

			if (strafeInput != null)
			{
				strafeInput.started -= OnStrafeStarted;
				strafeInput.ended -= OnStrafeEnded;
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
		}

		//Protected Methods
		/// <summary>
		/// Handles player landing
		/// </summary>
		protected virtual void OnLanding()
		{
			aerialState = ThirdPersonAerialMovementState.Grounded;
			//cachedForwardMovement = 0f;

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
			if (!characterPhysics.isGrounded || characterPhysics.startedSlide || !animationController.CanJump ||
				movementState == ThirdPersonGroundMovementState.TurningAround)
			{
				return;
			}

			aerialState = ThirdPersonAerialMovementState.Jumping;

			if (jumpStarted != null)
			{
				jumpStarted();
			}

			if (Mathf.Abs(normalizedLateralSpeed) < normalizedForwardSpeed)
			{
				characterPhysics.SetJumpVelocity(configuration.initialJumpVelocity);
				cachedForwardMovement = averageForwardMovement.average;
			}
		}

		/// <summary>
		/// Method called by strafe input started
		/// </summary>
		protected virtual void OnStrafeStarted()
		{
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

			thirdPersonBrain.thirdPersonCameraAnimationManager.StrafeEnded();
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
			Vector3 lookForwardY = transform.rotation.eulerAngles;

			lookForwardY.x = 0;
			lookForwardY.z = 0;
			lookForwardY.y -= characterInput.lookInput.x * Time.deltaTime * configuration.scaleStrafeLook;

			Quaternion targetRotation = Quaternion.Euler(lookForwardY);

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
			
			rotator.Tick();

			Quaternion newRotation = rotator.GetNewRotation(transform, targetRotation, turnSpeed);

			SetTurningSpeed(transform.rotation, newRotation);

			transform.rotation = newRotation;
		}

		protected virtual void SetStartStrafeLookDirection()
		{
			var cameraForward = Camera.main.transform.forward;
			cameraForward.y = 0;

			Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
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
				thirdPersonBrain.thirdPersonCameraAnimationManager.StrafeStarted();
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
			
			// TODO HACK TO INCREASE SPRINT SPEED
			if (sprint && normalizedForwardSpeed > 1)
			{
				animationController.unityAnimator.speed = sprintAnimatorSpeed;
			}
			else
			{
				float speed = 1;
				// check if we are performing an animation turnaround as this also changes animator speed
				if (movementState == ThirdPersonGroundMovementState.TurningAround)
				{
					var t = thirdPersonBrain.turnaround as AnimationTurnaroundBehaviour;
					if (t != null)
					{
						speed = t.currentAnimatorSpeed;
					}
				}
				animationController.unityAnimator.speed = speed;
			}

			if (characterPhysics.isGrounded)
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
			Vector3 flatForward = cameraTransform.forward;
			flatForward.y = 0f;
			flatForward.Normalize();

			Vector3 localMovementDirection =
				new Vector3(characterInput.moveInput.x, 0f, characterInput.moveInput.y);
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
					Debug.Log(deltaMagnitude);
					previousInputs.Clear();
					return true;
				}
			}
			angle = 0;
			return false;
		}
	}
}
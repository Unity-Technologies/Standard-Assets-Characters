using System;
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

		protected SlidingAverage strafeAverageForwardInput, strafeAverageLateralInput;

		private bool isTurningIntoStrafe;
		protected Transform transform;
		protected GameObject gameObject;
		protected ThirdPersonBrain thirdPersonBrain;

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
			strafeAverageForwardInput = new SlidingAverage(configuration.strafeInputWindowSize);
			strafeAverageLateralInput = new SlidingAverage(configuration.strafeInputWindowSize);

			if (cameraTransform == null)
			{
				cameraTransform = Camera.main.transform;
			}

			if (strafeInput != null)
			{
				strafeInput.Init();
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

			//Turnaround subscription for runtime support
			foreach (TurnaroundBehaviour turnaroundBehaviour in thirdPersonBrain.turnaroundOptions)
			{
				turnaroundBehaviour.turnaroundComplete += TurnaroundComplete;
			}
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

			//Turnaround un-subscription for runtime support
			foreach (TurnaroundBehaviour turnaroundBehaviour in thirdPersonBrain.turnaroundOptions)
			{
				turnaroundBehaviour.turnaroundComplete -= TurnaroundComplete;
			}
		}

		public void Update()
		{
			HandleMovement();
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
			if (!characterPhysics.isGrounded || !animationController.shouldUseRootMotion)
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

			Quaternion newRotation =
				Quaternion.RotateTowards(transform.rotation, targetRotation,
				                         turnSpeed * Time.deltaTime);

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
			normalizedLateralSpeed = 0;
			normalizedForwardSpeed = Mathf.Clamp(characterInput.moveInput.magnitude, -1, 1);

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


		protected virtual float DecelerateClampSpeed(float currentValue, float targetValue, float gain)
		{
			if (currentValue <= targetValue)
			{
				return targetValue;
			}

			return Mathf.Lerp(currentValue, targetValue, Time.deltaTime * gain);
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

			float currentY = transform.eulerAngles.y;
			float newY = target.eulerAngles.y;
			float angle = MathUtilities.Wrap180(MathUtilities.Wrap180(newY) - MathUtilities.Wrap180(currentY));

			if (ShouldTurnAround(angle))
			{
				cachedForwardMovement = averageForwardMovement.average;
				preTurnMovementState = movementState;
				movementState = ThirdPersonGroundMovementState.TurningAround;
				thirdPersonBrain.turnaround.TurnAround(angle);
				return true;
			}

			return false;
		}

		protected virtual bool ShouldTurnAround(float angle)
		{
			if (Mathf.Approximately(normalizedForwardSpeed, 0))
			{
				return Mathf.Abs(angle) > configuration.angleRapidTurn;
			}
			
			return Vector2.Angle(characterInput.moveInput, characterInput.previousNonZeroMoveInput) >
			       configuration.angleRapidTurn;
		}
	}
}
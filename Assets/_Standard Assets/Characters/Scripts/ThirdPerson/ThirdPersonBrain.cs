using System;
using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Helpers;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(IThirdPersonInput))]
	public class ThirdPersonBrain : CharacterBrain
	{
		/// <summary>
		/// Character animation states
		/// </summary>
		public enum AnimatorState
		{
			Locomotion,
			PhysicsJump,
			Falling,
			Landing
		}
		
		[SerializeField, Tooltip("Set to true if you do not want to use the Camera animation manager")]
		protected bool useSimpleCameras;
		
		[SerializeField, Tooltip("Properties of the root motion motor")]
		protected ThirdPersonMotor motor;
		
		[SerializeField]
		protected TurnaroundType turnaroundType;

		[SerializeField]
		protected BlendspaceTurnaroundBehaviour blendspaceTurnaroundBehaviour;

		[SerializeField]
		protected AnimationTurnaroundBehaviour animationTurnaroundBehaviour;
		
		[SerializeField]
		protected ThirdPersonMovementEventHandler thirdPersonMovementEventHandler;
				
		private const float k_HeadTurnSnapBackScale = 100f;

		[SerializeField, Tooltip("Configuration settings for the animator")]
		protected AnimationConfig configuration;

		// Hashes of the animator parameters
		private int hashForwardSpeed;
		private int hashLateralSpeed;
		private int hashTurningSpeed;
		private int hashVerticalSpeed;
		private int hashGroundedFootRight;
		private int hashJumpedForwardSpeed;
		private int hashJumpedLateralSpeed;
		private int hashFall;
		private int hashStrafe;
		private int hashSpeedMultiplier;

		// is the character grounded
		private bool isGrounded;

		// was the last physics jump taken during a planted right foot
		private bool lastPhysicsJumpRightRoot;

		// angle of the head for look direction
		private float headAngle;

		// cached default animator speed
		private float cachedAnimatorSpeed = 1.0f;

		// time of the last physics jump
		private float timeOfLastPhysicsJumpLand;

		// whether locomotion mode is set to strafe
		private bool isStrafing;

		private bool isTryingStrafe;

		//the camera controller to be used
		private ThirdPersonCameraController cameraController;
		private bool isChangingCamera;

		private bool triggeredRapidDirectionChange;
		private int framesToWait;

		private IThirdPersonInput input;

		private TurnaroundBehaviour[] turnaroundBehaviours;
		
		private float rapidStrafeTime, rapidStrafeChangeDuration;
		
		public ThirdPersonMotor thirdPersonMotor
		{
			get { return motor; }
		}
		
		public TurnaroundType typeOfTurnaround
		{
			get { return turnaroundType; }
		}

		public TurnaroundBehaviour turnaround { get; private set; }

		public TurnaroundBehaviour[] turnaroundOptions
		{
			get
			{
				if (turnaroundBehaviours == null)
				{
					turnaroundBehaviours = new TurnaroundBehaviour[]
					{
						blendspaceTurnaroundBehaviour, 
						animationTurnaroundBehaviour
					};
				}
				return turnaroundBehaviours;
			}
		}

		public override float normalizedForwardSpeed
		{
			get { return motor.normalizedForwardSpeed; }
		}

		public override float targetYRotation { get; set; }

		public ThirdPersonCameraController thirdPersonCameraController
		{
			get
			{
				return cameraController;
			}
		}

		public IThirdPersonInput thirdPersonInput
		{
			get
			{
				if (input == null)
				{
					input = GetComponent<IThirdPersonInput>();
				}

				return input;
			}
		}

		/// <summary>
		/// Gets the animation state of the character.
		/// </summary>
		/// <value>The current <see cref="AnimatorState"/></value>
		public AnimatorState animatorState { get; private set; }

		/// <summary>
		/// Gets the character animator.
		/// </summary>
		/// <value>The Unity Animator component</value>
		public Animator animator { get; private set; }

		/// <summary>
		/// Gets the animator forward speed.
		/// </summary>
		/// <value>The animator forward speed parameter.</value>
		public float animatorForwardSpeed
		{
			get { return animator.GetFloat(hashForwardSpeed); }
		}

		/// <summary>
		/// Gets the animator turning speed.
		/// </summary>
		/// <value>The animator forward speed parameter.</value>
		public float animatorTurningSpeed
		{
			get { return animator.GetFloat(hashTurningSpeed); }
		}

		/// <summary>
		/// Gets whether the right foot is planted
		/// </summary>
		/// <value>True is the right foot is planted; false if the left.</value>
		public bool isRightFootPlanted { get; private set; }

		/// <summary>
		/// Gets whether the character in a root motion state.
		/// </summary>
		/// <value>True if the state is in a grounded state; false if aerial.</value>
		public bool isRootMotionState
		{
			get
			{
				return animatorState == AnimatorState.Locomotion ||
				       animatorState == AnimatorState.Landing;
			}
		}
		
		/// <summary>
		/// The value of the animator lateral speed parameter.
		/// </summary>
		private float animatorLateralSpeed
		{
			get { return animator.GetFloat(hashLateralSpeed); }
		}

		/// <summary>
		/// Called on the exit of the land animation.
		/// </summary>
		/// <remarks>Should only be called by a land StateMachineBehaviour</remarks>
		public void OnLandAnimationExit()
		{
			if (isGrounded)
			{
				animatorState = AnimatorState.Locomotion;
			}

			animator.speed = cachedAnimatorSpeed;
		}

		/// <summary>
		/// Called on the enter of the land animation.
		/// </summary>
		/// <remarks>Should only be called by a land StateMachineBehaviour</remarks>
		/// <param name="adjustAnimationSpeedBasedOnForwardSpeed">Should the animator speed be adjusted during the land animation</param>
		public void OnLandAnimationEnter(bool adjustAnimationSpeedBasedOnForwardSpeed)
		{
			animatorState = AnimatorState.Landing;
			if (adjustAnimationSpeedBasedOnForwardSpeed)
			{
				animator.speed = configuration.landSpeedAsAFactorSpeed.Evaluate(motor.normalizedForwardSpeed);
			}
		}

		/// <summary>
		/// Called on the exit of the physics jump animation.
		/// </summary>
		/// <remarks>Should only be called by a physics jump StateMachineBehaviour</remarks>
		public void OnPhysicsJumpAnimationExit()
		{
			if (animatorState == AnimatorState.PhysicsJump)
			{
				animatorState = AnimatorState.Locomotion;
			}
		}

		/// <summary>
		/// Called on the enter of the physics jump animation.
		/// </summary>
		/// <remarks>Should only be called by a physics jump StateMachineBehaviour</remarks>
		public void OnPhysicsJumpAnimationEnter()
		{
			animatorState = AnimatorState.PhysicsJump;
		}

		/// <summary>
		/// Called on the enter of the locomotion animation.
		/// </summary>
		/// <remarks>Should only be called by a locomotion StateMachineBehaviour</remarks>
		public void OnLocomotionAnimationEnter()
		{
			if (animatorState == AnimatorState.Falling)
			{
				animatorState = AnimatorState.Locomotion;
			}
		}

		/// <summary>
		/// Called on the enter of the falling animation.
		/// </summary>
		/// <remarks>Should only be called by a falling StateMachineBehaviour</remarks>
		public void OnFallingLoopAnimationEnter()
		{
			animatorState = AnimatorState.Falling;
		}

		/// <summary>
		/// Update the animator forward speed parameter.
		/// </summary>
		/// <param name="newSpeed">New forward speed</param>
		/// <param name="deltaTime">Interpolation delta time</param>
		public void UpdateForwardSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashForwardSpeed, newSpeed,
			                  configuration.forwardSpeedInterpolation.GetInterpolationTime(animatorForwardSpeed, 
			                                                        newSpeed), deltaTime);
		}

		/// <summary>
		/// Update the animator turning speed parameter.
		/// </summary>
		/// <param name="newSpeed">New turning speed</param>
		/// <param name="deltaTime">Interpolation delta time</param>
		public void UpdateTurningSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashTurningSpeed, newSpeed,
			                  configuration.turningSpeedInterpolation.GetInterpolationTime(animatorTurningSpeed, 
			                                                        newSpeed),deltaTime);
		}
		
		protected override void Awake()
		{
			base.Awake();
			blendspaceTurnaroundBehaviour.Init(this);
			animationTurnaroundBehaviour.Init(this);
			turnaround = GetCurrentTurnaroundBehaviour();
			motor.Init(this);
			
			InitAnimator();
			
			thirdPersonMovementEventHandler.Init(this);
			CheckCameraAnimationManager();
		}
		
		protected override void Update()
		{
			base.Update();

			UpdateAnimatorParameters();
			
			motor.Update();

			if (turnaround != null)
			{
				turnaround.Update();
			}
			
			targetYRotation = motor.targetYRotation;
		
			//Just for build testing
			//TODO remove
			if (Input.GetKeyDown(KeyCode.T))
			{
				turnaroundType = turnaroundType == TurnaroundType.Animation ? TurnaroundType.None : turnaroundType + 1;
				turnaround = GetCurrentTurnaroundBehaviour();
			}
		}
		
		/// <summary>
		/// Sets the Animator parameters.
		/// </summary>
		private void UpdateAnimatorParameters()
		{
			UpdateTurningSpeed(motor.normalizedTurningSpeed, Time.deltaTime);

			bool fullyGrounded = isGrounded && animatorState != AnimatorState.Landing;
			// only update during landing if there is input to inhibit a jarring stop post land animation.
			bool landingWithInput = animatorState == AnimatorState.Landing &&
			                        (CheckHasSpeed(motor.normalizedForwardSpeed) ||
			                         CheckHasSpeed(motor.normalizedLateralSpeed));

			if (fullyGrounded || landingWithInput
			                  || animatorState == AnimatorState.Falling
			) // update during falling as landing animation depends on forward speed.
			{
				UpdateAnimationMovementSpeeds(Time.deltaTime);
				UpdateFoot();
			}
			else
			{
				animator.SetFloat(hashVerticalSpeed, motor.normalizedVerticalSpeed);
			}
		}

		/// <summary>
		/// Checks if <see cref="ThirdPersonCameraController"/> has been assigned - otherwise looks for it in the scene
		/// </summary>
		private void CheckCameraAnimationManager()
		{
			if (useSimpleCameras)
			{
				return;
			}
			
			if (cameraController == null)
			{
				ThirdPersonCameraController[] cameraControllers =
					FindObjectsOfType<ThirdPersonCameraController>();

				int length = cameraControllers.Length; 
				if (length != 1)
				{
					string errorMessage = "No ThirdPersonCameraAnimationManagers in scene! Disabling Brain";
					if (length > 1)
					{
						errorMessage = "Too many ThirdPersonCameraAnimationManagers in scene! Disabling Brain";
					}
					Debug.LogError(errorMessage);
					gameObject.SetActive(false);
					return;
				}

				cameraController = cameraControllers[0];
			}
			
			cameraController.SetThirdPersonBrain(this);
		}

		private TurnaroundBehaviour GetCurrentTurnaroundBehaviour()
		{
			switch (turnaroundType)
			{
					case TurnaroundType.None:
						return null;
					case TurnaroundType.Blendspace:
						return blendspaceTurnaroundBehaviour;
					case TurnaroundType.Animation:
						return animationTurnaroundBehaviour;
					default:
						return null;
			}
		}

		private void OnEnable()
		{
			controllerAdapter.jumpVelocitySet += thirdPersonMovementEventHandler.Jumped;
			controllerAdapter.landed += thirdPersonMovementEventHandler.Landed;
			controllerAdapter.landed += OnLanding;
				
			motor.jumpStarted += OnJumpStarted;
			motor.fallStarted += OnFallStarted;
			motor.Subscribe();

			if (thirdPersonInput != null)
			{
				ThirdPersonInput userInput = thirdPersonInput as ThirdPersonInput;
				if (userInput != null)
				{
					userInput.jumpPressed += motor.OnJumpPressed;
					userInput.sprintStarted += motor.OnSprintStarted;
					userInput.sprintEnded += motor.OnSprintEnded;
					userInput.strafeStarted += OnStrafeStarted;
					userInput.strafeEnded += OnStrafeEnded;
					userInput.recentreCamera += RecenterCamera;
				}
			}
			
			thirdPersonMovementEventHandler.Subscribe();
		}
		
		private void OnDisable()
		{
			if (controllerAdapter != null)
			{
				controllerAdapter.jumpVelocitySet -= thirdPersonMovementEventHandler.Jumped;
				controllerAdapter.landed -= thirdPersonMovementEventHandler.Landed;
				controllerAdapter.landed -= OnLanding;
			}
			
			if (motor != null)
			{
				if (thirdPersonInput != null)
				{
					ThirdPersonInput userInput = thirdPersonInput as ThirdPersonInput;
					if (userInput != null)
					{
						userInput.jumpPressed -= motor.OnJumpPressed;
						userInput.sprintStarted -= motor.OnSprintStarted;
						userInput.sprintEnded -= motor.OnSprintEnded;
						userInput.strafeStarted -= OnStrafeStarted;
						userInput.strafeEnded -= OnStrafeEnded;
						userInput.recentreCamera -= RecenterCamera;
					}
				}
				
				motor.jumpStarted -= OnJumpStarted;
				motor.fallStarted -= OnFallStarted;
				motor.Unsubscribe();
			}
			thirdPersonMovementEventHandler.Unsubscribe();
		}

		private void OnAnimatorMove()
		{
			motor.OnAnimatorMove();
		}

		private void OnAnimatorIK(int layerIndex)
		{
			HeadTurn();
		}
		
		/// <summary>
		/// Handles the head turning.
		/// </summary>
		private void HeadTurn()
		{
			if (!configuration.enableHeadLookAt)
			{
				return;
			}

			if (motor.currentAerialMovementState != ThirdPersonAerialMovementState.Grounded &&
			    !configuration.lookAtWhileAerial)
			{
				return;
			}

			if (motor.currentGroundMovementState == ThirdPersonGroundMovementState.TurningAround &&
			    !configuration.lookAtWhileTurnaround)
			{
				return;
			}

			animator.SetLookAtWeight(configuration.lookAtWeight);
			float targetHeadAngle = Mathf.Clamp((motor.targetYRotation - gameObject.transform.eulerAngles.y).Wrap180(),
				-configuration.lookAtMaxRotation, configuration.lookAtMaxRotation);

			float headTurn = Time.deltaTime * configuration.lookAtRotationSpeed;

			if (motor.currentGroundMovementState == ThirdPersonGroundMovementState.TurningAround)
			{
				if (Mathf.Abs(targetHeadAngle) < Mathf.Abs(headAngle))
				{
					headTurn *= k_HeadTurnSnapBackScale;
				}
				else
				{
					headTurn *= motor.currentTurnaroundBehaviour.headTurnScale;
				}
			}

			headAngle = Mathf.LerpAngle(headAngle, targetHeadAngle, headTurn);

			Vector3 lookAtPos = animator.transform.position +
			                    Quaternion.AngleAxis(headAngle, Vector3.up) * animator.transform.forward * 100f;
			animator.SetLookAtPosition(lookAtPos);
		}
		
		/// <summary>
		/// Gets the required components.
		/// </summary>
		private void InitAnimator()
		{
			hashForwardSpeed = Animator.StringToHash(AnimationControllerInfo.k_ForwardSpeedParameter);
			hashLateralSpeed = Animator.StringToHash(AnimationControllerInfo.k_LateralSpeedParameter);
			hashTurningSpeed = Animator.StringToHash(AnimationControllerInfo.k_TurningSpeedParameter);
			hashVerticalSpeed = Animator.StringToHash(AnimationControllerInfo.k_VerticalSpeedParameter);
			hashGroundedFootRight = Animator.StringToHash(AnimationControllerInfo.k_GroundedFootRightParameter);
			hashJumpedForwardSpeed = Animator.StringToHash(AnimationControllerInfo.k_JumpedForwardSpeedParameter);
			hashJumpedLateralSpeed = Animator.StringToHash(AnimationControllerInfo.k_JumpedLateralSpeedParameter);
			hashStrafe = Animator.StringToHash(AnimationControllerInfo.k_StrafeParameter);
			hashFall = Animator.StringToHash(AnimationControllerInfo.k_FallParameter);
			hashSpeedMultiplier = Animator.StringToHash(AnimationControllerInfo.k_SpeedMultiplier);
			animator = GetComponent<Animator>();
			cachedAnimatorSpeed = animator.speed;
		}

		/// <summary>
		/// Fires when the <see cref="motor"/> enters the fall state.
		/// </summary>
		private void OnFallStarted(float predictedFallDistance)
		{
			isGrounded = false;
			animator.SetTrigger(hashFall);
		}

		/// <summary>
		/// Logic for dealing with animation on landing
		/// Fires when the <see cref="motor"/> enters a rapid turn.
		/// </summary>
		private void OnLanding()
		{
			isGrounded = true;
			SelectGroundedCamera();

			switch (animatorState)
			{
				// if coming from a physics jump handle animation transition
				case AnimatorState.PhysicsJump:
					bool rightFoot = animator.GetBool(hashGroundedFootRight);
					float duration = configuration.jumpEndTransitionByForwardSpeed.Evaluate(
						Mathf.Abs(animator.GetFloat(AnimationControllerInfo.k_JumpedForwardSpeedParameter)));
					string locomotion = isStrafing
						                    ? AnimationControllerInfo.k_StrafeLocomotionState
						                    : AnimationControllerInfo.k_LocomotionState;
					animator.CrossFadeInFixedTime(locomotion, duration, 0, rightFoot
						                                                       ? configuration
							                                                       .rightFootPhysicsJumpLandAnimationOffset
						                                                       : configuration
							                                                       .leftFootPhysicsJumpLandAnimationOffset);
					timeOfLastPhysicsJumpLand = Time.time;
					break;
				case AnimatorState.Falling:
					// strafe mode does not have a landing animation so transition directly to locomotion
					if (isStrafing)
					{
						animator.CrossFade(AnimationControllerInfo.k_StrafeLocomotionState,
						                   configuration.landAnimationBlendDuration);
					}
					else
					{
						if (motor.normalizedForwardSpeed > configuration.forwardSpeedToRoll
						) // moving fast enough to roll
						{
							if (motor.fallTime > configuration.fallTimeRequiredToRoll) // play roll
							{
								animator.CrossFade(AnimationControllerInfo.k_RollLandState,
								                   configuration.rollAnimationBlendDuration);
							}
							else // has not fallen for long enough to roll
							{
								animator.CrossFade(AnimationControllerInfo.k_LocomotionState,
								                   configuration.landAnimationBlendDuration);
							}
						}
						else // play land 
						{
							animator.CrossFade(AnimationControllerInfo.k_LandState, configuration.landAnimationBlendDuration);
						}
					}

					break;
			}
		}
		
		private void UpdateAnimationMovementSpeeds(float deltaTime)
		{
			if (motor.movementMode == ThirdPersonMotorMovementMode.Strafe && 
			    new Vector2(animatorLateralSpeed, animatorForwardSpeed).magnitude > 0.5f)
			{
				if (triggeredRapidDirectionChange)
				{
					rapidStrafeTime += deltaTime;
					float progress = configuration.strafeRapidChangeSpeedCurve.Evaluate(
						rapidStrafeTime / rapidStrafeChangeDuration);
					float current = Mathf.Clamp(1.0f - (2.0f * progress), -1.0f, 1.0f);
					animator.SetFloat(hashSpeedMultiplier, current);

					CheckForStrafeRapidDirectionChangeComplete(deltaTime);
					return;
				}

				// check if a rapid direction change has occured.
				float angle = (Vector2.Angle(input.moveInput, new Vector2(animatorLateralSpeed, animatorForwardSpeed)));
				if (angle >= configuration.strafeRapidChangeAngleThreshold)
				{
					triggeredRapidDirectionChange = !CheckForStrafeRapidDirectionChangeComplete(deltaTime);
					rapidStrafeTime = 0.0f;
					return;
				}
			}

			// not rapid strafe direction change, update as normal
			animator.SetFloat(hashLateralSpeed, motor.normalizedLateralSpeed,
			                  configuration.lateralSpeedInterpolation.GetInterpolationTime(animatorLateralSpeed, 
			                                                                               motor.normalizedLateralSpeed),
			                  deltaTime);
			UpdateForwardSpeed(motor.normalizedForwardSpeed, deltaTime);
		}

		private bool CheckForStrafeRapidDirectionChangeComplete(float deltaTime)
		{
			if (IsNormalizedTimeCloseToZeroOrHalf(deltaTime, out rapidStrafeChangeDuration))
			{
				animator.SetFloat(hashLateralSpeed, -animatorLateralSpeed);
				animator.SetFloat(hashForwardSpeed, -animatorForwardSpeed);
				triggeredRapidDirectionChange = false;
				animator.SetFloat(hashSpeedMultiplier, 1.0f);;
				return true;
			}
			return false;
		}

		private void RecenterCamera()
		{
			thirdPersonCameraController.RecenterCamera();
		}
		
		/// <summary>
		/// Sets the animator strafe parameter to true.
		/// </summary>
		private void OnStrafeStarted()
		{
			isChangingCamera = true;
			isTryingStrafe = true;
			if (controllerAdapter.isGrounded)
			{
				SelectGroundedCamera();
			}
		}
		/// <summary>
		/// Sets the animator strafe parameter to false.
		/// </summary>
		private void OnStrafeEnded()
		{
			isChangingCamera = true;
			isTryingStrafe = false;
			if (controllerAdapter.isGrounded)
			{
				SelectGroundedCamera();
			}
		}
		
		private void SelectGroundedCamera()
		{
			if (!isChangingCamera)
			{
				return;
			}
			
			isStrafing = isTryingStrafe;
			animator.SetBool(hashStrafe, isStrafing);
			if (isStrafing)
			{
				motor.StartStrafe();
				cameraController.SetStrafeCamera();
			}
			else
			{
				motor.EndStrafe();
				cameraController.SetExplorationCamera();
			}

			isChangingCamera = false;
		}

		/// <summary>
		/// Logic for dealing with animation on jumping
		/// Fires when the <see cref="motor"/> enters a jump.
		/// </summary>
		private void OnJumpStarted()
		{
			if (!isGrounded)
			{
				return;
			}

			isGrounded = false;

			float movementMagnitude = new Vector2(animatorLateralSpeed, animatorForwardSpeed).magnitude;
			SetJumpForward(animatorForwardSpeed);
			bool rightFoot = animator.GetBool(hashGroundedFootRight);
			
			float duration = configuration.jumpTransitionDurationFactorOfSpeed.Evaluate(movementMagnitude);
			// keep track of the last jump so legs can be alternated if necessary. ie a skip.
			if (timeOfLastPhysicsJumpLand + configuration.skipJumpWindow >= Time.time)
			{
				rightFoot = !lastPhysicsJumpRightRoot;
			}
			animator.SetFloat(hashJumpedLateralSpeed, 0.0f);
			animator.CrossFade(rightFoot ? 
				                   AnimationControllerInfo.k_RightFootJumpState : 
				                   AnimationControllerInfo.k_LeftFootJumpState, duration);
			lastPhysicsJumpRightRoot = rightFoot;
		}

		private bool IsNormalizedTimeCloseToZeroOrHalf(float margin, out float timeUntilZeroOrHalf)
		{
			float value = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;

			timeUntilZeroOrHalf = (value >= 0.5 ? 1.0f : 0.5f) - value;
			
			return (value > 1.0f - margin || value < margin ||
			        (value > 0.5f - margin && value < 0.5f + margin));
		}

		private void SetJumpForward(float jumpForward)
		{
			jumpForward = jumpForward.Remap01(configuration.standingJumpNormalizedSpeedThreshold,
			                                  configuration.runningJumpNormalizedSpeedThreshold);
			animator.SetFloat(hashJumpedForwardSpeed, jumpForward);
		}

		/// <summary>
		/// Uses the normalized progress of the animation to determine the grounded foot.
		/// </summary>
		private void UpdateFoot()
		{
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			var animationNormalizedProgress = stateInfo.normalizedTime.GetFraction();
			//TODO: remove zero index
			if ((animationNormalizedProgress + configuration.groundedFootThresholdOffsetValue).Wrap1() >
			    (configuration.groundedFootThresholdValue + configuration.groundedFootThresholdOffsetValue).Wrap1())
			{
				SetGroundedFootRight(!configuration.invertFoot);
				return;
			}

			SetGroundedFootRight(configuration.invertFoot);
		}

		/// <summary>
		/// Sets the grounded foot of the animator. This is used to play the appropriate footed animations.
		/// </summary>
		private void SetGroundedFootRight(bool value)
		{
			if (Mathf.Abs(motor.normalizedLateralSpeed) < Mathf.Epsilon)
			{
				animator.SetBool(hashGroundedFootRight, value);
				isRightFootPlanted = value;
				return;
			}

			// while strafing a foot is preferred depending on lateral direction
			bool lateralSpeedRight = motor.normalizedLateralSpeed < 0.0f;
			animator.SetBool(hashGroundedFootRight, lateralSpeedRight);
			isRightFootPlanted = lateralSpeedRight;
		}

		private static bool CheckHasSpeed(float speed)
		{
			return Mathf.Abs(speed) > 0.0f;
		}
		
#if UNITY_EDITOR
		private void OnValidate()
		{
			turnaround = GetCurrentTurnaroundBehaviour();
		}
#endif
	}
	
	/// <summary>
	/// Handles the third person movement event triggers and event IDs.
	/// As well as collider movement detections <see cref="ColliderMovementDetection"/>
	/// </summary>
	[Serializable]
	public class ThirdPersonMovementEventHandler : MovementEventHandler
	{
		/// <summary>
		/// The movement detection colliders attached to the feet of the Third Person Character
		/// </summary>
		[SerializeField]
		protected ColliderMovementDetection leftFootDetection;
		
		[SerializeField]
		protected ColliderMovementDetection rightFootDetection;

		[SerializeField]
		protected float maximumSpeed = 10f;

		private ThirdPersonBrain thirdPersonBrain;

		/// <summary>
		/// Gives the <see cref="ThirdPersonMovementEventHandler"/> context of the <see cref="ThirdPersonBrain"/>
		/// </summary>
		/// <param name="brainToUse">The <see cref="ThirdPersonBrain"/> that called Init</param>
		public void Init(ThirdPersonBrain brainToUse)
		{
			base.Init(brainToUse);
			thirdPersonBrain = brainToUse;
		}

		/// <summary>
		/// Subscribe to the movement detection events
		/// </summary>
		public void Subscribe()
		{
			if (leftFootDetection != null)
			{
				leftFootDetection.detection += HandleLeftFoot;
			}
			if (rightFootDetection != null)
			{
				rightFootDetection.detection += HandleRightFoot;
			}
		}

		/// <summary>
		/// Unsubscribe to the movement detection events
		/// </summary>
		public void Unsubscribe()
		{
			if (leftFootDetection != null)
			{
				leftFootDetection.detection -= HandleLeftFoot;
			}
			if (rightFootDetection != null)
			{
				rightFootDetection.detection -= HandleRightFoot;
			}
		}

		/// <summary>
		/// Plays the Jumping movement events
		/// </summary>
		public void Jumped()
		{
			PlayJumping(new MovementEventData(brain.transform));
		}

		/// <summary>
		/// Plays the landing movement events
		/// </summary>
		public void Landed()
		{
			PlayLanding(new MovementEventData(brain.transform));
		}

		/// <summary>
		/// Plays the left foot movement events
		/// </summary>
		/// <param name="movementEventData">the data need to play the event</param>
		private void HandleLeftFoot(MovementEventData movementEventData)
		{
			movementEventData.normalizedSpeed = Mathf.Clamp01(thirdPersonBrain.planarSpeed/maximumSpeed);
			PlayLeftFoot(movementEventData);
		}
		
		/// <summary>
		/// Plays the right foot movement events
		/// </summary>
		/// <param name="movementEventData">the data need to play the event</param>
		private void HandleRightFoot(MovementEventData movementEventData)
		{
			movementEventData.normalizedSpeed = Mathf.Clamp01(thirdPersonBrain.planarSpeed/maximumSpeed);
			PlayRightFoot(movementEventData);
		}
	}
}
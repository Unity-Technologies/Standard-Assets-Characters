using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Helpers;
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
			RootMotionJump,
			Falling,
			Landing
		}
		
		[HelperBox(HelperBoxAttribute.HelperType.Info,
			"Configurations are separate assets (ScriptableObjects). Click on the associated configuration to locate it in the Project View. Values can be edited here during runtime and not be lost. It also allows one to create different settings and swap between them. To create a new setting Right click -> Create -> Standard Assets -> Characters -> ...")]
		
		[SerializeField, Tooltip("Set to true if you do not want to use the Camera animation manager"), DisableEditAtRuntime()]
		protected bool useSimpleCameras;
		
		[SerializeField, VisibleIf("useSimpleCameras",false), Tooltip("The camera animation manager to use")]
		protected ThirdPersonCameraController cameraController;
		
		[SerializeField, Tooltip("Properties of the root motion motor")]
		protected ThirdPersonMotor motor;
		
		[SerializeField]
		protected TurnaroundType turnaroundType;

		[SerializeField]
		[VisibleIf("turnaroundType", TurnaroundType.Blendspace)]
		protected BlendspaceTurnaroundBehaviour blendspaceTurnaroundBehaviour;

		[SerializeField]
		[VisibleIf("turnaroundType", TurnaroundType.Animation)]
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

		private bool triggeredRapidDirectionChange;
		private int framesToWait;

		private const float k_StrafeRapidDirectionChangeRangeMargin = 0.025f,
		                    k_StrafeRapidDirectionChangeRangeStartInMargin = 0.05f;

		private IThirdPersonInput input;

		private TurnaroundBehaviour[] turnaroundBehaviours;
		
		public ThirdPersonMotor thirdPersonMotor
		{
			get { return motor; }
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

		/// <inheritdoc/>
		public override float normalizedForwardSpeed
		{
			get { return motor.normalizedForwardSpeed; }
		}

		public override MovementEventHandler movementEventHandler
		{
			get { return thirdPersonMovementEventHandler; }
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
		/// Gets the current root motion movement modifier.
		/// </summary>
		public Vector3 currentRootMotionModifier { get; private set; }

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
				       animatorState == AnimatorState.RootMotionJump ||
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
			if (animatorState == AnimatorState.RootMotionJump || animatorState == AnimatorState.Falling)
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
		/// Update the animator lateral speed parameter.
		/// </summary>
		/// <param name="newSpeed">New lateral speed</param>
		/// <param name="deltaTime">Interpolation delta time</param>
		public void UpdateLateralSpeed(float newSpeed, float deltaTime)
		{
			if (triggeredRapidDirectionChange)
			{
				if (framesToWait-- > 0)
				{
					return;
				}

				if (IsNormalizedTimeCloseToZeroOrHalf(k_StrafeRapidDirectionChangeRangeMargin))
				{
					animator.SetFloat(hashLateralSpeed, -animatorLateralSpeed);
					triggeredRapidDirectionChange = false;
				}
				currentRootMotionModifier = Vector3.one;
				return;
			}

			// check if a rapid direction change has occured.
			float delta = Mathf.Abs(thirdPersonInput.moveInput.x - animatorLateralSpeed);
			if (delta >= configuration.strafeRapidChangeThreshold)
			{
				triggeredRapidDirectionChange = true;
				// if we instant change within the viable range there is a pop so wait a few frames 
				if (IsNormalizedTimeCloseToZeroOrHalf(k_StrafeRapidDirectionChangeRangeStartInMargin))
				{
					framesToWait = configuration.strafeRapidDirectionFrameWaitCount;
				}
				currentRootMotionModifier = new Vector3(-1.0f, 1.0f, 1.0f);
				return;
			}

			animator.SetFloat(hashLateralSpeed, newSpeed,
			                  configuration.lateralSpeedInterpolation.GetInterpolationTime(animatorLateralSpeed, 
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
				UpdateForwardSpeed(motor.normalizedForwardSpeed, Time.deltaTime);
				UpdateLateralSpeed(motor.normalizedLateralSpeed, Time.deltaTime);
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
				Debug.Log("No ThirdPersonCameraController set up. Searching scene...");
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
			physicsForCharacter.jumpVelocitySet += thirdPersonMovementEventHandler.Jumped;
			physicsForCharacter.landed += thirdPersonMovementEventHandler.Landed;
				
			motor.jumpStarted += OnJumpStarted;
			motor.landed += OnLanding;
			motor.fallStarted += OnFallStarted;
			motor.Subscribe();

			if (thirdPersonCameraController != null)
			{
				thirdPersonCameraController.forwardLockedModeStarted += OnStrafeStarted;
				thirdPersonCameraController.forwardUnlockedModeStarted += OnStrafeEnded;
			}
			
			thirdPersonMovementEventHandler.Subscribe();
		}
		
		private void OnDisable()
		{
			if (motor != null)
			{
				motor.jumpStarted -= OnJumpStarted;
				motor.landed -= OnLanding;
				motor.fallStarted -= OnFallStarted;
				motor.Unsubscribe();
			}

			if (thirdPersonCameraController != null)
			{
				thirdPersonCameraController.forwardLockedModeStarted -= OnStrafeStarted;
				thirdPersonCameraController.forwardUnlockedModeStarted -= OnStrafeEnded;
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
			animator = GetComponent<Animator>();
			cachedAnimatorSpeed = animator.speed;
			currentRootMotionModifier = Vector3.one;
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

		/// <summary>
		/// Sets the animator strafe parameter to true.
		/// </summary>
		private void OnStrafeStarted()
		{
			isStrafing = true;
			animator.SetBool(hashStrafe, isStrafing);
		}

		/// <summary>
		/// Sets the animator strafe parameter to false.
		/// </summary>
		private void OnStrafeEnded()
		{
			isStrafing = false;
			animator.SetBool(hashStrafe, isStrafing);
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

			float jumpForward = animatorForwardSpeed;
			SetJumpForward(jumpForward);
			bool rightFoot = animator.GetBool(hashGroundedFootRight);

			// is it a root motion or physics jump
			if (Mathf.Abs(motor.normalizedLateralSpeed) <= Mathf.Abs(motor.normalizedForwardSpeed)
			    && motor.normalizedForwardSpeed >= 0.0f) // forward jump: physics
			{
				float duration = configuration.jumpTransitionDurationFactorOfSpeed.Evaluate(jumpForward);
				// keep track of the last jump so legs can be alternated if necessary. ie a skip.
				if (timeOfLastPhysicsJumpLand + configuration.skipJumpWindow >= Time.time)
				{
					rightFoot = !lastPhysicsJumpRightRoot;
				}

				animator.SetFloat(hashJumpedLateralSpeed, 0.0f);
				animator.CrossFade(
					rightFoot ? AnimationControllerInfo.k_RightFootJumpState : AnimationControllerInfo.k_LeftFootJumpState, duration);
				lastPhysicsJumpRightRoot = rightFoot;
			}
			else // lateral or backwards jump;: root motion
			{
				// disallow diagonal jumps
				if (Mathf.Abs(motor.normalizedForwardSpeed) > Mathf.Abs(motor.normalizedLateralSpeed))
				{
					animator.SetFloat(hashJumpedForwardSpeed, motor.normalizedForwardSpeed);
					animator.SetFloat(hashJumpedLateralSpeed, 0.0f);
				}
				else
				{
					animator.SetFloat(hashJumpedLateralSpeed, motor.normalizedLateralSpeed);
					animator.SetFloat(hashJumpedForwardSpeed, 0.0f);
				}

				animator.CrossFadeInFixedTime(rightFoot
					                              ? AnimationControllerInfo.k_RightFootRootMotionJumpState
					                              : AnimationControllerInfo.k_LeftFootRootMotionJumpState,
				                              configuration.rootMotionJumpCrossfadeDuration);
				animatorState = AnimatorState.RootMotionJump;
			}
		}

		private bool IsNormalizedTimeCloseToZeroOrHalf(float margin)
		{
			float value = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;
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
}
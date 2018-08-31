using System;
using StandardAssets.Characters.Physics;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Character animation states
	/// </summary>
	public enum AnimationState
	{
		Locomotion,
		PhysicsJump,
		RootMotionJump,
		Falling,
		Landing
	}
	/// <summary>
	/// Class that sends Third Person locomotion to the Animator.
	/// </summary>
	[Serializable]
	public class ThirdPersonAnimationController
	{
		private const float k_HeadTurnSnapBackScale = 100f;

		[SerializeField, Tooltip("Configuration settings for the animator")]
		protected ThirdPersonAnimationConfiguration configuration;

		/// <summary>
		/// Required motor
		/// </summary>
		private IThirdPersonMotor motor;

		/// <summary>
		/// The animator
		/// </summary>
		private Animator animator;
		
		/// <summary>
		/// The character's GameObject
		/// </summary>
		private GameObject gameObject;

		/// <summary>
		/// Hashes of the animator parameters
		/// </summary>
		private int hashForwardSpeed;
		private int hashLateralSpeed;
		private int hashTurningSpeed;
		private int hashVerticalSpeed;
		private int hashGrounded;
		private int hashHasInput;
		private int hashFootedness;
		private int hashJumpedForwardSpeed;
		private int hashJumpedLateralSpeed;
		private int hashRapidTurn;
		private int hashFall;

		// is the character grounded
		private bool isGrounded;
		// was the last physics jump taken during a planted right foot
		private bool lastPhysicsJumpRightRoot;
		// angle of the head for look direction
		private float headAngle;
		// cached default animator speed
		private float cachedAnimatorSpeed = 1;
		// time of the last physics jump
		private float timeOfLastPhysicsJumpLand;

		/// <summary>
		/// Gets the animation state of the character.
		/// </summary>
		/// <value>The current <see cref="AnimationState"/></value>
		public AnimationState state { get; private set; }

		/// <summary>
		/// Gets the character animator.
		/// </summary>
		/// <value>The Unity Animator component</value>
		public Animator unityAnimator
		{
			get { return animator; }
		}
		
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
		/// The value of the animator lateral speed parameter.
		/// </summary>
		private float animatorLateralSpeed
		{
			get { return animator.GetFloat(hashLateralSpeed); }
		}

		/// <summary>
		/// Gets whether the right foot is planted
		/// </summary>
		/// <value>True is the right foot is planted; false if the left.</value>
		public bool isRightFootPlanted { get; private set; }

		/// <summary>
		/// Gets whether the character in a root motion state.
		/// </summary>
		/// <value>True if the state is in a grounded state; false if airborne.</value>
		public bool isRootMotionState
		{
			get { return state == AnimationState.Locomotion || 
						  state == AnimationState.RootMotionJump ||
						  state == AnimationState.Landing; }
		}

		/// <summary>
		/// Called on the exit of the land animation.
		/// </summary>
		/// <remarks>Should only be called by a land StateMachineBehaviour</remarks>
		public void OnLandAnimationExit()
		{
			if (isGrounded)
			{
				state = AnimationState.Locomotion;
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
			state = AnimationState.Landing;
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
			if (state == AnimationState.PhysicsJump)
			{
				state = AnimationState.Locomotion;
			}
		}

		/// <summary>
		/// Called on the enter of the physics jump animation.
		/// </summary>
		/// <remarks>Should only be called by a physics jump StateMachineBehaviour</remarks>
		public void OnPhysicsJumpAnimationEnter()
		{
			state = AnimationState.PhysicsJump;
		}

		/// <summary>
		/// Called on the enter of the locomotion animation.
		/// </summary>
		/// <remarks>Should only be called by a locomotion StateMachineBehaviour</remarks>
		public void OnLocomotionAnimationEnter()
		{
			if (state == AnimationState.RootMotionJump || state == AnimationState.Falling)
			{
				state = AnimationState.Locomotion;
			}
		}

		/// <summary>
		/// Called on the enter of the falling animation.
		/// </summary>
		/// <remarks>Should only be called by a falling StateMachineBehaviour</remarks>
		public void OnFallingLoopAnimationEnter()
		{
			state = AnimationState.Falling;
		}

		/// <summary>
		/// Update the animator forward speed parameter.
		/// </summary>
		/// <param name="newSpeed">New forward speed</param>
		/// <param name="deltaTime">Interpolation delta time</param>
		public void UpdateForwardSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashForwardSpeed, newSpeed,
							  configuration.forwardSpeed.GetInterpolationTime(animatorForwardSpeed, newSpeed),
							  deltaTime);
		}

		/// <summary>
		/// Update the animator lateral speed parameter.
		/// </summary>
		/// <param name="newSpeed">New lateral speed</param>
		/// <param name="deltaTime">Interpolation delta time</param>
		public void UpdateLateralSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashLateralSpeed, newSpeed,
							  configuration.lateralSpeed.GetInterpolationTime(animatorLateralSpeed, newSpeed),
							  deltaTime);
		}

		/// <summary>
		/// Update the animator turning speed parameter.
		/// </summary>
		/// <param name="newSpeed">New turning speed</param>
		/// <param name="deltaTime">Interpolation delta time</param>
		public void UpdateTurningSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashTurningSpeed, newSpeed,
							  configuration.turningSpeed.GetInterpolationTime(animatorTurningSpeed, newSpeed),
							  deltaTime);
		}

		/// <summary>
		/// Gets the required components.
		/// </summary>
		public void Init(ThirdPersonBrain brain, IThirdPersonMotor motorToUse)
		{
			gameObject = brain.gameObject;
			hashForwardSpeed = Animator.StringToHash(configuration.forwardSpeed.parameter);
			hashLateralSpeed = Animator.StringToHash(configuration.lateralSpeed.parameter);
			hashTurningSpeed = Animator.StringToHash(configuration.turningSpeed.parameter);
			hashVerticalSpeed = Animator.StringToHash(configuration.verticalSpeedParameterName);
			hashGrounded = Animator.StringToHash(configuration.groundedParameterName);
			hashHasInput = Animator.StringToHash(configuration.hasInputParameterName);
			hashFootedness = Animator.StringToHash(configuration.footednessParameterName);
			hashJumpedForwardSpeed = Animator.StringToHash(configuration.jumpedForwardSpeedParameterName);
			hashJumpedLateralSpeed = Animator.StringToHash(configuration.jumpedLateralSpeedParameterName);
			hashRapidTurn = Animator.StringToHash(configuration.rapidTurnParameterName);
			hashFall = Animator.StringToHash(configuration.fallParameterName);
			motor = motorToUse;
			animator = gameObject.GetComponent<Animator>();
			cachedAnimatorSpeed = animator.speed;
		}

		/// <summary>
		/// Sets the Animator parameters.
		/// </summary>
		public void Update()
		{
			UpdateTurningSpeed(motor.normalizedTurningSpeed, Time.deltaTime);
		
			animator.SetBool(hashHasInput,
							 CheckHasSpeed(motor.normalizedForwardSpeed) ||
							 CheckHasSpeed(motor.normalizedLateralSpeed));

			bool fullyGrounded = isGrounded && state != AnimationState.Landing;
			// only update during landing if there is input to inhibit a jarring stop post land animation.
			bool landingWithInput = state == AnimationState.Landing &&
									(CheckHasSpeed(motor.normalizedForwardSpeed) ||
									 CheckHasSpeed(motor.normalizedLateralSpeed));
			
			if (fullyGrounded || landingWithInput
				|| state == AnimationState.Falling) // update during falling as landing animation depends on forward speed.
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
		/// Handles the head turning.
		/// </summary>
		public void HeadTurn()
		{
			if (configuration.disableHeadLookAt)
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
			float targetHeadAngle = Mathf.Clamp(
				MathUtilities.Wrap180(motor.targetYRotation - gameObject.transform.eulerAngles.y),
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
		/// Subscribe to motor events.
		/// </summary>
		/// <remarks>Should be called by OnEnable</remarks>
		public void Subscribe()
		{
			motor.jumpStarted += OnJumpStarted;
			motor.landed += OnLanding;
			motor.fallStarted += OnFallStarted;
			motor.rapidlyTurned += OnRapidlyTurned;
		}

		/// <summary>
		/// Unsubscribe from events.
		/// </summary>
		/// <remarks>Should be called by OnDisable</remarks>
		public void Unsubscribe()
		{
			if (motor != null)
			{
				motor.jumpStarted -= OnJumpStarted;
				motor.landed -= OnLanding;
				motor.fallStarted -= OnFallStarted;
				motor.rapidlyTurned -= OnRapidlyTurned;
			}
		}

		/// <summary>
		/// Fires when the <see cref="motor"/> enters the fall state.
		/// </summary>
		private void OnFallStarted(float predictedFallDistance)
		{
			isGrounded = false;
			animator.SetBool(hashGrounded, false);
			animator.SetTrigger(hashFall);
		}

		/// <summary>
		/// Fires when the <see cref="motor"/> enters a rapid turn.
		/// </summary>
		private void OnRapidlyTurned(float normalizedTurn)
		{
			animator.SetTrigger(hashRapidTurn);
		}

		/// <summary>
		/// Logic for dealing with animation on landing
		/// Fires when the <see cref="motor"/> enters a rapid turn.
		/// </summary>
		private void OnLanding()
		{
			isGrounded = true;

			switch (state)
			{
				// if coming from a physics jump handle animation transition
				case AnimationState.PhysicsJump:
					bool rightFoot = animator.GetBool(hashFootedness);
					float duration = configuration.jumpEndTransitionByForwardSpeed.Evaluate(
						Mathf.Abs(animator.GetFloat(configuration.jumpedForwardSpeedParameterName)));
					animator.CrossFadeInFixedTime(configuration.locomotionStateName, duration, 0, rightFoot ? 
										  configuration.rightFootPhysicsJumpLandAnimationOffset
										: configuration.leftFootPhysicsJumpLandAnimationOffset);
					timeOfLastPhysicsJumpLand = Time.time;
					break;
				case AnimationState.Falling:
					// strafe mode does not have a landing animation so transition directly to locomotion
					var rootMotionMotor = motor as RootMotionThirdPersonMotor;
					if (rootMotionMotor != null && rootMotionMotor.movementMode == ThirdPersonMotorMovementMode.Strafe)
					{
						animator.CrossFade(configuration.locomotionStateName, configuration.landAnimationBlendDuration);
					}
					else
					{
						if (motor.normalizedForwardSpeed > configuration.forwardSpeedToRoll) // play roll
						{
							animator.CrossFade(configuration.rollLandStateName, configuration.rollAnimationBlendDuration);
						}
						else // play land 
						{
							animator.CrossFade(configuration.landStateName, configuration.landAnimationBlendDuration);
						}
					}
					break;
			}
			animator.SetBool(hashGrounded, true);
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
			// TODO any non zero standing jump will blend into moving jump which is ugly. This should be fixed by new animation
			if (jumpForward < 0.1f)
			{
				jumpForward = 0;
			}
			animator.SetFloat(hashJumpedForwardSpeed, jumpForward);

			bool rightFoot = animator.GetBool(hashFootedness);

			float duration = configuration.jumpTransitionDurationFactorOfSpeed.Evaluate(jumpForward);
			// is it a root motion or physics jump
			if (Mathf.Abs(motor.normalizedLateralSpeed) <= Mathf.Abs(motor.normalizedForwardSpeed)
				&& motor.normalizedForwardSpeed >= 0) // forward jump: physics
			{
				// keep track of the last jump so legs can be alternated if necessary. ie a skip.
				if (timeOfLastPhysicsJumpLand + configuration.skipJumpWindow >= Time.time)
				{
					rightFoot = !lastPhysicsJumpRightRoot;
				}
				animator.SetFloat(hashJumpedLateralSpeed, 0);
				animator.CrossFade(rightFoot ? configuration.rightFootJumpStateName :
											   configuration.leftFootJumpStateName, duration);
				lastPhysicsJumpRightRoot = rightFoot;
			}
			else // lateral or backwards jump;: root motion
			{
				animator.SetFloat(hashJumpedLateralSpeed, motor.normalizedLateralSpeed);
				animator.CrossFade(rightFoot ? configuration.rightFootRootMotionJumpStateName 
											 : configuration.leftFootRootMotionJumpStateName, duration);
				state = AnimationState.RootMotionJump;
			}

			animator.SetBool(hashGrounded, false);
		}

		/// <summary>
		/// Uses the normalized progress of the animation to determine footedness.
		/// </summary>
		private void UpdateFoot()
		{
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			var animationNormalizedProgress = MathUtilities.GetFraction(stateInfo.normalizedTime);
			//TODO: remove zero index
			if (MathUtilities.Wrap1(animationNormalizedProgress +
									configuration.footednessThresholdOffsetValue) >
				MathUtilities.Wrap1(configuration.footednessThresholdValue +
									configuration.footednessThresholdOffsetValue))
			{
				SetFootednessBool(!configuration.invertFoot);
				return;
			}

			SetFootednessBool(configuration.invertFoot);
		}
		
		/// <summary>
		/// Sets the footedness of the animator. This is used to play the appropriate footed animations.
		/// </summary>
		private void SetFootednessBool(bool value)
		{
			if (Mathf.Abs(motor.normalizedLateralSpeed) < Mathf.Epsilon)
			{
				animator.SetBool(hashFootedness, value);
				isRightFootPlanted = value;
				return;
			}

			// while strafing a foot is preferred depending on lateral direction
			bool lateralSpeedRight = motor.normalizedLateralSpeed < 0;
			animator.SetBool(hashFootedness, lateralSpeedRight);
			isRightFootPlanted = lateralSpeedRight;
		}

		private static bool CheckHasSpeed(float speed)
		{
			return Mathf.Abs(speed) > 0;
		}
	}
}
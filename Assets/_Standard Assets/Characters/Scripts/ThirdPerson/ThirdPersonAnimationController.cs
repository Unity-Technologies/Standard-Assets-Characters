using System;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	public enum AnimationState
	{
		Locomotion,
		PhysicsJump,
		RootMotionJump,
		Falling,
		Landing
	}
	/// <summary>
	/// Class that sends Third Person locomotion to the Animator 
	/// </summary>
	[Serializable]
	public class ThirdPersonAnimationController
	{
		
		private const float k_HeadTurnSnapBackScale = 100f;

		[SerializeField]
		protected ThirdPersonAnimationConfiguration configuration;

		/// <summary>
		/// Required motor
		/// </summary>
		private IThirdPersonMotor motor;

		/// <summary>
		/// The animator
		/// </summary>
		private Animator animator;

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
		private int hashFallingTime;
		private int hashFootedness;
		private int hashJumpedForwardSpeed;
		private int hashJumpedLateralSpeed;
		private int hashPredictedFallDistance;
		private int hashRapidTurn;

		private bool isGrounded,
					 lastPhysicsJumpRightRoot;

		private float headAngle;
		private float animationNormalizedProgress;
		private float cachedAnimatorSpeed = 1;
		private DateTime timeSinceLastPhysicsJumpLand;

		public AnimationState state { get; private set; }

		public Animator unityAnimator
		{
			get { return animator; }
		}

		public float animatorForwardSpeed
		{
			get { return animator.GetFloat(hashForwardSpeed); }
		}

		private float animatorLateralSpeed
		{
			get { return animator.GetFloat(hashLateralSpeed); }
		}

		public float animatorTurningSpeed
		{
			get { return animator.GetFloat(hashTurningSpeed); }
		}

		public bool isRightFootPlanted { get; private set; }

		public bool canJump
		{
			get { return state == AnimationState.Locomotion && isGrounded; }
		}

		public void OnLandAnimationExit()
		{
			state = AnimationState.Locomotion;
		}

		public void OnLandAnimationEnter()
		{
			state = AnimationState.Landing;
		}

		public void OnPhysicsJumpAnimationExit()
		{
			if (state == AnimationState.PhysicsJump)
			{
				state = AnimationState.Locomotion;
			}
		}

		public void OnPhysicsJumpAnimationEnter()
		{
			state = AnimationState.PhysicsJump;
		}

		public void OnLocomotionAnimationEnter()
		{
			if (state == AnimationState.RootMotionJump)
			{
				state = AnimationState.Locomotion;
			}
			animator.SetFloat(configuration.predictedFallDistanceParameterName, 0);
			animator.speed = cachedAnimatorSpeed;
		}

		public void OnFallingLoopAnimationEnter()
		{
			state = AnimationState.Falling;
		}

		public void UpdateForwardSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashForwardSpeed, newSpeed,
							  configuration.forwardSpeed.GetInterpolationTime(animatorForwardSpeed, newSpeed),
							  deltaTime);
		}

		public void UpdateLateralSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashLateralSpeed, newSpeed,
							  configuration.lateralSpeed.GetInterpolationTime(animatorLateralSpeed, newSpeed),
							  deltaTime);
		}

		public void UpdateTurningSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashTurningSpeed, newSpeed,
							  configuration.turningSpeed.GetInterpolationTime(animatorTurningSpeed, newSpeed),
							  deltaTime);
		}

		/// <summary>
		/// Gets the required components
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
			hashFallingTime = Animator.StringToHash(configuration.fallingTimeParameterName);
			hashFootedness = Animator.StringToHash(configuration.footednessParameterName);
			hashJumpedForwardSpeed = Animator.StringToHash(configuration.jumpedForwardSpeedParameterName);
			hashJumpedLateralSpeed = Animator.StringToHash(configuration.jumpedLateralSpeedParameterName);
			hashPredictedFallDistance = Animator.StringToHash(configuration.predictedFallDistanceParameterName);
			hashRapidTurn = Animator.StringToHash(configuration.rapidTurnParameterName);
			motor = motorToUse;
			animator = gameObject.GetComponent<Animator>();
			cachedAnimatorSpeed = animator.speed;
		}

		/// <summary>
		/// Sets the Animator parameters
		/// </summary>
		public void Update()
		{
			UpdateTurningSpeed(motor.normalizedTurningSpeed, Time.deltaTime);

			animator.SetBool(hashHasInput,
							 CheckHasSpeed(motor.normalizedForwardSpeed) ||
							 CheckHasSpeed(motor.normalizedLateralSpeed));


			if ((isGrounded || state == AnimationState.Falling) && state != AnimationState.Landing)
			{
				UpdateForwardSpeed(motor.normalizedForwardSpeed, Time.deltaTime);
				UpdateLateralSpeed(motor.normalizedLateralSpeed, Time.deltaTime);
				UpdateFoot();
			}
			else
			{
				animator.SetFloat(hashVerticalSpeed, motor.normalizedVerticalSpeed);
				animator.SetFloat(hashFallingTime, motor.fallTime);
			}
		}

		/// <summary>
		/// Handles the head turning
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
		/// Subscribe
		/// </summary>
		public void Subscribe()
		{
			motor.jumpStarted += OnJumpStarted;
			motor.landed += OnLanding;
			motor.fallStarted += OnFallStarted;
			motor.rapidlyTurned += OnRapidlyTurned;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
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

		private void OnFallStarted(float predictedFallDistance)
		{
			isGrounded = false;
			animator.SetFloat(hashFallingTime, 0);
			animator.SetBool(hashGrounded, false);
			animator.SetFloat(hashPredictedFallDistance, predictedFallDistance);
		}

		private void SetFootednessBool(bool value)
		{
			if (Mathf.Abs(motor.normalizedLateralSpeed) < Mathf.Epsilon)
			{
				animator.SetBool(hashFootedness, value);
				isRightFootPlanted = value;
				return;
			}

			bool lateralSpeedRight = motor.normalizedLateralSpeed < 0;
			animator.SetBool(hashFootedness, lateralSpeedRight);
			isRightFootPlanted = lateralSpeedRight;
		}

		private void OnRapidlyTurned(float normalizedTurn)
		{
			animator.SetTrigger(hashRapidTurn);
		}

		/// <summary>
		/// Logic for dealing with animation on landing
		/// </summary>
		private void OnLanding()
		{
			isGrounded = true;

			switch (state)
			{
				// if coming from a physics jump handle animation transition
				case AnimationState.PhysicsJump:
					bool rightFoot = animator.GetBool(hashFootedness);
					animator.CrossFade("Locomotion Blend", configuration.jumpTransitionDurationByForwardSpeed.Evaluate(
										Mathf.Abs(animator.GetFloat(configuration.jumpedForwardSpeedParameterName))),
										0, rightFoot ? configuration.rightFootPhysicsJumpLandAnimationOffset
										: configuration.leftFootPhysicsJumpLandAnimationOffset);
					timeSinceLastPhysicsJumpLand = DateTime.Now;
					break;
				// if falling set speed of land animation based on forward speed
				case AnimationState.Falling:
					animator.speed = configuration.landSpeedAsAFactorSpeed.Evaluate(motor.normalizedForwardSpeed);
					break;
			}
			animator.SetBool(hashGrounded, true);
		}

		/// <summary>
		/// Logic for dealing with animation on jumping
		/// </summary>
		private void OnJumpStarted()
		{
			if (!isGrounded)
			{
				return;
			}

			isGrounded = false;

			animator.SetFloat(hashJumpedForwardSpeed, motor.normalizedForwardSpeed);

			bool rightFoot = animator.GetBool(hashFootedness);

			if (timeSinceLastPhysicsJumpLand.AddSeconds(configuration.skipJumpWindow) >= DateTime.Now)
			{
				rightFoot = !lastPhysicsJumpRightRoot;
			}

			if (Mathf.Abs(motor.normalizedLateralSpeed) <= Mathf.Abs(motor.normalizedForwardSpeed)
				&& motor.normalizedForwardSpeed >= 0)
			{
				animator.SetFloat(hashJumpedLateralSpeed, 0);
				animator.CrossFade(rightFoot ? "OnRightFootBlend" : "OnLeftFootBlend",
								   configuration.jumpTransitionTime);
				lastPhysicsJumpRightRoot = rightFoot;
			}
			else
			{
				animator.SetFloat(hashJumpedLateralSpeed, motor.normalizedLateralSpeed);
				animator.CrossFade(rightFoot ? "OnRightFoot" : "OnLeftFoot",
								   configuration.jumpTransitionTime);
				state = AnimationState.RootMotionJump;
			}

			animator.SetFloat(hashFallingTime, 0);
			animator.SetBool(hashGrounded, false);
		}

		private void UpdateFoot()
		{
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			animationNormalizedProgress = MathUtilities.GetFraction(stateInfo.normalizedTime);
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

		private static bool CheckHasSpeed(float speed)
		{
			return Mathf.Abs(speed) > 0;
		}
	}
}
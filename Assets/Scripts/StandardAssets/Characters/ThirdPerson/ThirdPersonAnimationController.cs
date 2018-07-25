using System;
using Attributes;
using Attributes.Types;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Class that sends Third Person locomotion to the Animator 
	/// </summary>
	[Serializable]
	public class ThirdPersonAnimationController
	{
		[HelperBox(HelperType.Info,
			"Configuration is a separate asset. Click on the associated configuration to located it in the Project View. Values can be edited here during runtime and not be lost. It also allows one to create different settings and swap between them. To create a new setting Right click -> Create -> Standard Assets -> Characters -> Third Person Animation Configuration")]
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
		private int hashJumped;
		private int hashJumpedForwardSpeed;
		private int hashJumpedLateralSpeed;
		private int hashPredictedFallDistance;
		private int hashRapidTurn;
		private int hashIsStrafing;

		private bool isGrounded;

		public Animator unityAnimator
		{
			get { return animator; }
		}

		public float animatorForwardSpeed
		{
			get { return animator.GetFloat(hashForwardSpeed); }
		}

		public float animatorLateralSpeed
		{
			get { return animator.GetFloat(hashLateralSpeed); }
		}

		public float animatorTurningSpeed
		{
			get { return animator.GetFloat(hashTurningSpeed); }
		}

		public float animationNormalizedProgress { get; private set; }

		public float footednessNormalizedProgress
		{
			get
			{
				if (isRightFootPlanted)
				{
					return MathUtilities.Wrap1(
						animationNormalizedProgress - configuration.footednessThresholdOffsetValue -
						configuration.footednessThresholdValue);
				}

				return animationNormalizedProgress;
			}
		}

		public bool isRightFootPlanted { get; private set; }

		public bool shouldUseRootMotion { get; private set; }

		public void AirborneStateExit()
		{
			animator.SetFloat(configuration.predictedFallDistanceParameterName, 0);
			shouldUseRootMotion = true;
		}

		public void AirborneStateEnter()
		{
			if (motor.normalizedForwardSpeed > 0 && Mathf.Approximately(Mathf.Abs(motor.normalizedLateralSpeed), 0))
			{
				shouldUseRootMotion = false;
			}
		}

		public void LocomotionStateUpdate()
		{
			//TODO update locomotion pre run
		}

		public void UpdatePredictedFallDistance(float distance)
		{
			animator.SetFloat(hashPredictedFallDistance, distance);
		}

		public void UpdateForwardSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashForwardSpeed, newSpeed, configuration.floatInterpolationTime, deltaTime);
		}

		public void UpdateLateralSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashLateralSpeed, newSpeed, configuration.floatInterpolationTime, deltaTime);
		}

		public void UpdateTurningSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashTurningSpeed, newSpeed, configuration.floatInterpolationTime, deltaTime);
		}

		/// <summary>
		/// Gets the required components
		/// </summary>
		public void Init(ThirdPersonBrain brain, IThirdPersonMotor motorToUse)
		{
			gameObject = brain.gameObject;
			hashForwardSpeed = Animator.StringToHash(configuration.forwardSpeedParameterName);
			hashLateralSpeed = Animator.StringToHash(configuration.lateralSpeedParameterName);
			hashTurningSpeed = Animator.StringToHash(configuration.turningSpeedParameterName);
			hashVerticalSpeed = Animator.StringToHash(configuration.verticalSpeedParameterName);
			hashGrounded = Animator.StringToHash(configuration.groundedParameterName);
			hashHasInput = Animator.StringToHash(configuration.hasInputParameterName);
			hashFallingTime = Animator.StringToHash(configuration.fallingTimeParameterName);
			hashFootedness = Animator.StringToHash(configuration.footednessParameterName);
			hashJumped = Animator.StringToHash(configuration.jumpedParameterName);
			hashJumpedForwardSpeed = Animator.StringToHash(configuration.jumpedForwardSpeedParameterName);
			hashJumpedLateralSpeed = Animator.StringToHash(configuration.jumpedLateralSpeedParameterName);
			hashPredictedFallDistance = Animator.StringToHash(configuration.predictedFallDistanceParameterName);
			hashRapidTurn = Animator.StringToHash(configuration.rapidTurnParameterName);
			hashIsStrafing = Animator.StringToHash(configuration.isStrafingParameterName);
			motor = motorToUse;
			animator = gameObject.GetComponent<Animator>();
		}

		/// <summary>
		/// Sets the Animator parameters
		/// </summary>
		public void Update()
		{
			UpdateForwardSpeed(motor.normalizedForwardSpeed, Time.deltaTime);
			UpdateLateralSpeed(motor.normalizedLateralSpeed, Time.deltaTime);
			UpdateTurningSpeed(motor.normalizedTurningSpeed, Time.deltaTime);
			UpdateFoot();

			animator.SetBool(hashHasInput,
			                 CheckHasSpeed(motor.normalizedForwardSpeed) ||
			                 CheckHasSpeed(motor.normalizedLateralSpeed));

			animator.SetBool(hashIsStrafing, Mathf.Abs(motor.normalizedLateralSpeed) > 0);

			if (!isGrounded)
			{
				animator.SetFloat(hashVerticalSpeed, motor.normalizedVerticalSpeed,
				                  configuration.floatInterpolationTime, Time.deltaTime);
				animator.SetFloat(hashFallingTime, motor.fallTime);
			}
		}

		public void HeadTurn()
		{
			animator.SetLookAtWeight(configuration.lookAtWeight);
			float angle = Mathf.Clamp(MathUtilities.Wrap180(motor.targetYRotation - animator.transform.eulerAngles.y),
			                          -configuration.lookAtMaxRotation, configuration.lookAtMaxRotation);

			Vector3 lookAtPos = animator.transform.position +
			                    Quaternion.AngleAxis(angle, Vector3.up) * animator.transform.forward * 100f;
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
			animator.SetBool(hashGrounded, true);
		}

		/// <summary>
		/// Logic for dealing with animation on jumping
		/// </summary>
		private void OnJumpStarted()
		{
			isGrounded = false;
			animator.SetTrigger(hashJumped);
			animator.SetFloat(hashFallingTime, 0);
			animator.SetBool(hashGrounded, false);

			if (Mathf.Abs(motor.normalizedLateralSpeed) > Mathf.Abs(motor.normalizedForwardSpeed))
			{
				animator.SetFloat(hashJumpedForwardSpeed, 0);
				animator.SetFloat(hashJumpedLateralSpeed, motor.normalizedLateralSpeed);
			}
			else
			{
				animator.SetFloat(hashJumpedLateralSpeed, 0);
				animator.SetFloat(hashJumpedForwardSpeed, motor.normalizedForwardSpeed);
			}
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

		private bool CheckHasSpeed(float speed)
		{
			return Mathf.Abs(speed) > 0;
		}

		/// <summary>
		/// Helper function to get the component of velocity along an axis
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="velocity"></param>
		/// <returns></returns>
		private float GetVectorOnAxis(Vector3 axis, Vector3 vector)
		{
			float dot = Vector3.Dot(axis, vector.normalized);
			float val = dot * vector.magnitude;

			Debug.Log(val);
			return val;
		}
	}
}
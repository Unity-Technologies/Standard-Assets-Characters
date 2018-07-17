using System;
using StandardAssets.Characters.Effects;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Class that sends Third Person locomotion to the Animator 
	/// </summary>
	[RequireComponent(typeof(IThirdPersonMotor))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonAnimationController : MonoBehaviour
	{
		[SerializeField]
		protected float floatInterpolationTime = 0.05f;
		
		[SerializeField]
		protected string forwardSpeedParameterName = "ForwardSpeed";
		
		[SerializeField]
		protected string lateralSpeedParameterName = "LateralSpeed";
		
		[SerializeField]
		protected string turningSpeedParameterName = "TurningSpeed";

		[SerializeField]
		protected string verticalSpeedParameterName = "VerticalSpeed";
		
		[SerializeField]
		protected string groundedParameterName = "Grounded";
		
		[SerializeField]
		protected string hasInputParameterName = "HasInput";
		
		[SerializeField]
		protected string fallingTimeParameterName = "FallTime";
		
		[SerializeField]
		protected string footednessParameterName = "OnRightFoot";
		
		[SerializeField]
		protected string jumpedParameterName = "Jumped";
		
		[SerializeField]
		protected string jumpedLateralSpeedParameterName = "JumpedLateralSpeed";
		
		[SerializeField]
		protected string jumpedForwardSpeedParameterName = "JumpedForwardSpeed";

		[SerializeField]
		protected string predictedFallDistanceParameterName = "PredictedFallDistance";
		
		[SerializeField]
		protected string rapidTurnParameterName = "RapidTurn";

		[SerializeField]
		protected string isStrafingParameterName = "IsStrafing";
		
		[SerializeField]
		protected bool invert;

		[SerializeField]
		protected float footednessThreshold = 0.25f, footednessThresholdOffset = 0.25f;
		
		/// <summary>
		/// Required motor
		/// </summary>
		private IThirdPersonMotor motor;
		
		/// <summary>
		/// The animator
		/// </summary>
		private Animator animator;
		
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


		public void AirborneStateExit()
		{
			animator.SetFloat(predictedFallDistanceParameterName, 0);
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
			animator.SetFloat(hashForwardSpeed, newSpeed, floatInterpolationTime, deltaTime);
		}
		
		public void UpdateLateralSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashLateralSpeed, newSpeed, floatInterpolationTime, deltaTime);
		}
		
		public void UpdateTurningSpeed(float newSpeed, float deltaTime)
		{
			animator.SetFloat(hashTurningSpeed, newSpeed, floatInterpolationTime, deltaTime);
		}

		/// <summary>
		/// Gets the required components
		/// </summary>
		private void Awake()
		{
			hashForwardSpeed = Animator.StringToHash(forwardSpeedParameterName);
			hashLateralSpeed = Animator.StringToHash(lateralSpeedParameterName);
			hashTurningSpeed = Animator.StringToHash(turningSpeedParameterName);
			hashVerticalSpeed = Animator.StringToHash(verticalSpeedParameterName);
			hashGrounded = Animator.StringToHash(groundedParameterName);
			hashHasInput = Animator.StringToHash(hasInputParameterName);
			hashFallingTime = Animator.StringToHash(fallingTimeParameterName);
			hashFootedness = Animator.StringToHash(footednessParameterName);
			hashJumped = Animator.StringToHash(jumpedParameterName);
			hashJumpedForwardSpeed = Animator.StringToHash(jumpedForwardSpeedParameterName);
			hashJumpedLateralSpeed = Animator.StringToHash(jumpedLateralSpeedParameterName);
			hashPredictedFallDistance = Animator.StringToHash(predictedFallDistanceParameterName);
			hashRapidTurn = Animator.StringToHash(rapidTurnParameterName);
			hashIsStrafing = Animator.StringToHash(isStrafingParameterName);
			motor = GetComponent<IThirdPersonMotor>();
			animator = GetComponent<Animator>();
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		private void OnEnable()
		{
			motor.jumpStarted += OnJumpStarted;
			motor.landed += OnLanding;
			motor.fallStarted += OnFallStarted;
			motor.rapidlyTurned += OnRapidlyTurned;
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
				return;
			}
			animator.SetBool(hashFootedness, motor.normalizedLateralSpeed > 0);
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		private void OnDisable()
		{
			if (motor != null)
			{
				motor.jumpStarted -= OnJumpStarted;
				motor.landed -= OnLanding;
				motor.fallStarted -= OnFallStarted;
				motor.rapidlyTurned -= OnRapidlyTurned;
			}
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

		/// <summary>
		/// Sets the Animator parameters
		/// </summary>
		private void Update()
		{
			UpdateForwardSpeed(motor.normalizedForwardSpeed, Time.deltaTime);
			UpdateLateralSpeed(motor.normalizedLateralSpeed, Time.deltaTime);
			UpdateTurningSpeed(motor.normalizedTurningSpeed, Time.deltaTime);
			UpdateFoot();
			
			animator.SetBool(hashHasInput, CheckHasSpeed(motor.normalizedForwardSpeed) || CheckHasSpeed(motor.normalizedLateralSpeed));
			
			animator.SetBool(hashIsStrafing, Mathf.Abs(motor.normalizedLateralSpeed) > 0);

			if (!isGrounded)
			{
				animator.SetFloat(hashVerticalSpeed, motor.normalizedVerticalSpeed, floatInterpolationTime, Time.deltaTime);
				animator.SetFloat(hashFallingTime, motor.fallTime);
			}
		}

		private void UpdateFoot()
		{
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			float currentProgress = MathUtilities.GetFraction(stateInfo.normalizedTime);
			Debug.LogError(currentProgress);
			//TODO: remove zero index
			if (MathUtilities.Wrap1(currentProgress +
			                        footednessThresholdOffset) >
			    MathUtilities.Wrap1(footednessThreshold + footednessThresholdOffset))
			{
				SetFootednessBool(!invert);
				return;
			}
			
			SetFootednessBool(invert);
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
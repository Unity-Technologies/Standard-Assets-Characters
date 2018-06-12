using StandardAssets.Characters.Effects;
using UnityEngine;

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
		private string forwardSpeedParameterName = "ForwardSpeed";
		
		[SerializeField]
		private string lateralSpeedParameterName = "LateralSpeed";
		
		[SerializeField]
		private string turningSpeedParameterName = "TurningSpeed";
		
		[SerializeField]
		private string groundedParameterName = "Grounded";
		
		[SerializeField]
		private string hasInputParameterName = "HasInput";
		
		[SerializeField]
		private string fallingTimeParameterName = "FallingTime";
		
		[Header("Footedness")]
		[SerializeField]
		private string footednessParameterName = "OnRightFoot";
		
		[SerializeField]
		private bool invert;
		
		[SerializeField]
		private ColliderMovementDetection leftFoot, rightfoot;
		
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
		private int hashGrounded;
		private int hashHasInput;
		private int hashFallingTime;
		private int hashFootedness;

		/// <summary>
		/// Gets the required components
		/// </summary>
		private void Awake()
		{
			hashForwardSpeed = Animator.StringToHash(forwardSpeedParameterName);
			hashLateralSpeed = Animator.StringToHash(lateralSpeedParameterName);
			hashTurningSpeed = Animator.StringToHash(turningSpeedParameterName);
			hashGrounded = Animator.StringToHash(groundedParameterName);
			hashHasInput = Animator.StringToHash(hasInputParameterName);
			hashFallingTime = Animator.StringToHash(fallingTimeParameterName);
			hashFootedness = Animator.StringToHash(footednessParameterName);
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
			if (leftFoot != null && rightfoot != null)
			{
				leftFoot.detection += OnLeftFoot;
				rightfoot.detection += OnRightFoot;
			}
		}

		private void OnRightFoot(MovementEvent obj)
		{
			SetFootednessBool(!invert);
		}

		private void OnLeftFoot(MovementEvent obj)
		{
			SetFootednessBool(invert);
		}

		private void SetFootednessBool(bool value)
		{
			animator.SetBool(hashFootedness, value);
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		private void OnDisable()
		{
			if (motor == null)
			{
				return;
			}
			
			motor.jumpStarted -= OnJumpStarted;
			motor.landed -= OnLanding;

			if (leftFoot != null && rightfoot != null)
			{
				leftFoot.detection -= OnLeftFoot;
				rightfoot.detection -= OnRightFoot;
			}
		}
		
		/// <summary>
		/// Logic for dealing with animation on landing
		/// </summary>
		private void OnLanding()
		{
			animator.SetBool(hashGrounded, true);
		}

		/// <summary>
		/// Logic for dealing with animation on jumping
		/// </summary>
		private void OnJumpStarted()
		{
			animator.SetBool(hashGrounded, false);
		}

		/// <summary>
		/// Sets the Animator parameters
		/// </summary>
		private void Update()
		{
			animator.SetFloat(hashForwardSpeed, motor.normalizedForwardSpeed);
			animator.SetFloat(hashLateralSpeed, motor.normalizedLateralSpeed);
			animator.SetFloat(hashTurningSpeed, motor.normalizedTurningSpeed);
			animator.SetBool(hashHasInput, CheckHasSpeed(motor.normalizedForwardSpeed) || CheckHasSpeed(motor.normalizedLateralSpeed));
			animator.SetFloat(hashFallingTime, motor.fallTime);
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
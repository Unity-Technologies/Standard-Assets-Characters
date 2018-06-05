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
		public string forwardSpeedParameterName = "ForwardSpeed";
		public string lateralSpeedParameterName = "LateralSpeed";
		public string turningSpeedParameterName = "TurningSpeed";
		public string groundedParameterName = "Grounded";
		public string hasInputParameterName = "HasInput";
		
		/// <summary>
		/// Required motor
		/// </summary>
		IThirdPersonMotor m_Motor;
		
		/// <summary>
		/// The animator
		/// </summary>
		Animator m_Animator;
		
		/// <summary>
		/// Hashes of the animator parameters
		/// </summary>
		int m_HashForwardSpeed;
		int m_HashLateralSpeed;
		int m_HashTurningSpeed;
		int m_HashGrounded;
		int m_HashHasInput;

		/// <summary>
		/// Gets the required components
		/// </summary>
		void Awake()
		{
			m_HashForwardSpeed = Animator.StringToHash(forwardSpeedParameterName);
			m_HashLateralSpeed = Animator.StringToHash(lateralSpeedParameterName);
			m_HashTurningSpeed = Animator.StringToHash(turningSpeedParameterName);
			m_HashGrounded = Animator.StringToHash(groundedParameterName);
			m_HashHasInput = Animator.StringToHash(hasInputParameterName);
			m_Motor = GetComponent<IThirdPersonMotor>();
			m_Animator = GetComponent<Animator>();
		}

		/// <summary>
		/// Subscribe
		/// </summary>
		void OnEnable()
		{
			m_Motor.jumpStarted += OnJumpStarted;
			m_Motor.landed += OnLanding;
		}

		/// <summary>
		/// Unsubscribe
		/// </summary>
		void OnDisable()
		{
			if (m_Motor == null)
			{
				return;
			}
			
			m_Motor.jumpStarted -= OnJumpStarted;
			m_Motor.landed -= OnLanding;
		}

		/// <summary>
		/// Logic for dealing with animation on landing
		/// </summary>
		void OnLanding()
		{
			m_Animator.SetBool(m_HashGrounded, true);
		}

		/// <summary>
		/// Logic for dealing with animation on jumping
		/// </summary>
		void OnJumpStarted()
		{
			m_Animator.SetBool(m_HashGrounded, false);
		}

		/// <summary>
		/// Sets the Animator parameters
		/// </summary>
		void Update()
		{
			m_Animator.SetFloat(m_HashForwardSpeed, m_Motor.normalizedForwardSpeed);
			m_Animator.SetFloat(m_HashLateralSpeed, m_Motor.normalizedLateralSpeed);
			m_Animator.SetFloat(m_HashTurningSpeed, m_Motor.normalizedTurningSpeed);
			m_Animator.SetBool(m_HashHasInput, CheckHasSpeed(m_Motor.normalizedForwardSpeed) || CheckHasSpeed(m_Motor.normalizedLateralSpeed));
		}

		bool CheckHasSpeed(float speed)
		{
			return Mathf.Abs(speed) > 0;
		}
	}
}
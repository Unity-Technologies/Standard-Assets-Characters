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
		int m_Grounded;

		/// <summary>
		/// Gets the required components
		/// </summary>
		void Awake()
		{
			m_HashForwardSpeed = Animator.StringToHash(forwardSpeedParameterName);
			m_HashLateralSpeed = Animator.StringToHash(lateralSpeedParameterName);
			m_HashTurningSpeed = Animator.StringToHash(turningSpeedParameterName);
			m_Grounded = Animator.StringToHash(groundedParameterName);
			m_Motor = GetComponent<IThirdPersonMotor>();
			m_Motor.jumpStart += OnJumpStart;
			m_Motor.lands += OnLand;
			m_Animator = GetComponent<Animator>();
		}

		/// <summary>
		/// Logic for dealing with animation on landing
		/// </summary>
		void OnLand()
		{
			m_Animator.SetBool(m_Grounded, true);
		}

		/// <summary>
		/// Logic for dealing with animation on jumping
		/// </summary>
		void OnJumpStart()
		{
			m_Animator.SetBool(m_Grounded, false);
		}

		/// <summary>
		/// Sets the Animator parameters
		/// </summary>
		void Update()
		{
			m_Animator.SetFloat(m_HashForwardSpeed, m_Motor.forwardSpeed);
			m_Animator.SetFloat(m_HashLateralSpeed, m_Motor.lateralSpeed);
			m_Animator.SetFloat(m_HashTurningSpeed, m_Motor.turningSpeed);
		}
	}
}
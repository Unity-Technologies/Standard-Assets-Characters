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
		readonly int m_HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
		readonly int m_HashLateralSpeed = Animator.StringToHash("LateralSpeed");
		readonly int m_HashTurningSpeed = Animator.StringToHash("TurningSpeed");

		/// <summary>
		/// Gets the required components
		/// </summary>
		void Awake()
		{
			m_Motor = GetComponent<IThirdPersonMotor>();
			m_Animator = GetComponent<Animator>();
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
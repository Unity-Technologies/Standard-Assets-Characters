using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(ThirdPersonMotor))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonAnimationController : MonoBehaviour
	{
		ThirdPersonMotor m_Motor;
		Animator m_Animator;
		
		readonly int m_HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
		readonly int m_HashLateralSpeed = Animator.StringToHash("LateralSpeed");
		readonly int m_HashTurningSpeed = Animator.StringToHash("TurningSpeed");

		void Awake()
		{
			m_Motor = GetComponent<ThirdPersonMotor>();
			m_Animator = GetComponent<Animator>();
		}

		void Update()
		{
			m_Animator.SetFloat(m_HashForwardSpeed, m_Motor.forwardSpeed);
			m_Animator.SetFloat(m_HashLateralSpeed, m_Motor.lateralSpeed);
			m_Animator.SetFloat(m_HashTurningSpeed, m_Motor.turningSpeed);
		}
	}
}
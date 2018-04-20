using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavMeshThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		NavMeshAgent m_Agent;

		public float maxAngularSpeed, maxLateralSpeed, maxForwardSpeed;

		void Awake()
		{
			m_Agent = GetComponent<NavMeshAgent>();
		}

		public float turningSpeed
		{
			get { return m_Agent.angularSpeed/maxAngularSpeed; }
		}

		public float lateralSpeed
		{
			get { return m_Agent.velocity.x/maxLateralSpeed; }
		}

		public float forwardSpeed
		{
			get { return m_Agent.velocity.z/maxForwardSpeed; }
		}
	}
}
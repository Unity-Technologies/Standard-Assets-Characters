using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavMeshThirdPersonMotor : ThirdPersonMotor
	{
		NavMeshAgent m_Agent;

		public float maxAngularSpeed, maxLateralSpeed, maxForwardSpeed;

		void Awake()
		{
			m_Agent = GetComponent<NavMeshAgent>();
		}

		public override float turningSpeed
		{
			get { return m_Agent.angularSpeed/maxAngularSpeed; }
		}

		public override float lateralSpeed
		{
			get { return m_Agent.velocity.x/maxLateralSpeed; }
		}

		public override float forwardSpeed
		{
			get { return m_Agent.velocity.z/maxForwardSpeed; }
		}
	}
}
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
			get { return GetVelocityOnAxis(m_Agent.transform.right, m_Agent.velocity) / maxLateralSpeed; }
		}

		public float forwardSpeed
		{
			get { return GetVelocityOnAxis(m_Agent.transform.forward, m_Agent.velocity) / maxForwardSpeed; }
		}

		private float GetVelocityOnAxis(Vector3 axis, Vector3 velocity)
		{
			float dot = Vector3.Dot(axis, velocity);
			return dot * velocity.magnitude;
		}
	}
}
using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Maps a NavMeshAgent movement to values that the animator understands
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavMeshThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		/// <summary>
		/// The attached NavMeshAgent
		/// </summary>
		NavMeshAgent m_Agent;

		/// <summary>
		/// Max speed values used in normalization
		/// </summary>
		public float maxAngularSpeed, maxLateralSpeed, maxForwardSpeed;

		/// <summary>
		/// Get the NavMeshAgent on Awake
		/// </summary>
		void Awake()
		{
			m_Agent = GetComponent<NavMeshAgent>();
		}

		/// <inheritdoc />
		public float turningSpeed
		{
			//TODO: get actual values
			get { return 0f*m_Agent.angularSpeed/maxAngularSpeed; }
		}

		/// <inheritdoc />
		public float lateralSpeed
		{
			get { return GetVelocityOnAxis(m_Agent.transform.right, m_Agent.velocity) / maxLateralSpeed; }
		}

		/// <inheritdoc />
		public float forwardSpeed
		{
			get { return GetVelocityOnAxis(m_Agent.transform.forward, m_Agent.velocity) / maxForwardSpeed; }
		}

		/// <summary>
		/// Helper function to get the component of velocity along an axis
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="velocity"></param>
		/// <returns></returns>
		private float GetVelocityOnAxis(Vector3 axis, Vector3 velocity)
		{
			float dot = Vector3.Dot(axis, velocity.normalized);
			float val = dot * velocity.magnitude;
			
			Debug.Log(val);
			return val;
		}
	}
}
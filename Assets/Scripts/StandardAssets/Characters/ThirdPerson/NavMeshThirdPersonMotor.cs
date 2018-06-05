using System;
using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Maps a NavMeshAgent movement to values that the animator understands
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavMeshThirdPersonMotor : BaseThirdPersonMotor
	{
		/// <summary>
		/// The attached NavMeshAgent
		/// </summary>
		NavMeshAgent m_Agent;

		/// <inheritdoc />
		public override float normalizedLateralSpeed
		{
			get { return GetVelocityOnAxis(m_Agent.transform.right, m_Agent.velocity) / m_Agent.speed; }
		}

		/// <inheritdoc />
		public override float normalizedForwardSpeed
		{
			get { return GetVelocityOnAxis(m_Agent.transform.forward, m_Agent.velocity) / m_Agent.speed; }
		}

		/// <summary>
		/// Get the NavMeshAgent on Awake
		/// </summary>
		protected override void Awake()
		{
			m_Agent = GetComponent<NavMeshAgent>();
			base.Awake();
		}

		/// <summary>
		/// Helper function to get the component of velocity along an axis
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="velocity"></param>
		/// <returns></returns>
		float GetVelocityOnAxis(Vector3 axis, Vector3 velocity)
		{
			float dot = Vector3.Dot(axis, velocity.normalized);
			float val = dot * velocity.magnitude;
			
			Debug.Log(val);
			return val;
		}

		/// <summary>
		/// Calculates the rotation about Y
		/// </summary>
		void Update()
		{
			CalculateYRotationSpeed();
		}
	}
}
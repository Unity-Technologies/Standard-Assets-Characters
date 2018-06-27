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
		private NavMeshAgent agent;

		private float fallingTime;

		/// <inheritdoc />
		public override float normalizedLateralSpeed
		{
			get { return GetVelocityOnAxis(agent.transform.right, agent.velocity) / agent.speed; }
		}

		/// <inheritdoc />
		public override float normalizedForwardSpeed
		{
			get { return GetVelocityOnAxis(agent.transform.forward, agent.velocity) / agent.speed; }
		}

		/// <inheritdoc />
		public override float fallTime
		{
			get { return fallingTime; }
		}

		/// <inheritdoc />
		protected override void Awake()
		{
			agent = GetComponent<NavMeshAgent>();
			fallingTime = 0f;
			base.Awake();
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

		/// <summary>
		/// Calculates the rotation about Y
		/// </summary>
		private void Update()
		{
			CalculateYRotationSpeed(Time.deltaTime);
		}
	}
}
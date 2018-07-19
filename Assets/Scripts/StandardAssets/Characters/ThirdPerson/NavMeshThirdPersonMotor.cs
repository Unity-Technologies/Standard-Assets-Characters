using System;
using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Maps a NavMeshAgent movement to values that the animator understands
	/// </summary>
	[Serializable]
	public class NavMeshThirdPersonMotor : IThirdPersonMotor
	{
		private GameObject gameObject;
		/// <summary>
		/// The attached NavMeshAgent
		/// </summary>
		private NavMeshAgent agent;

		private float fallingTime;

		public float normalizedTurningSpeed { get; private set; }

		/// <inheritdoc />
		public float normalizedLateralSpeed
		{
			get { return GetVelocityOnAxis(agent.transform.right, agent.velocity) / agent.speed; }
		}

		/// <inheritdoc />
		public float normalizedForwardSpeed
		{
			get { return GetVelocityOnAxis(agent.transform.forward, agent.velocity) / agent.speed; }
		}

		/// <inheritdoc />
		public float fallTime
		{
			get { return fallingTime; }
		}
		
		public float normalizedVerticalSpeed
		{
			get { return 0; }
		}

		public void Init(ThirdPersonBrain brain)
		{
			gameObject = brain.gameObject;
			agent = gameObject.GetComponent<NavMeshAgent>();
			fallingTime = 0f;
		}

		public Action jumpStarted { get; set; }
		public Action landed { get; set; }
		public Action<float> fallStarted { get; set; }
		public Action<float> rapidlyTurned { get; set; }

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
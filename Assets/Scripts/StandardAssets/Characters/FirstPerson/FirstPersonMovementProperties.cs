using System;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// The state that modifies the behaviour of the first person motor
	/// e.g. the difference between standing and crouching
	/// </summary>
	[Serializable]
	public class FirstPersonMovementProperties
	{
		/// <summary>
		/// The maximum movement speed
		/// </summary>
		[SerializeField]
		private float maxSpeed = 5f;

		/// <summary>
		/// The curve evaluator for acceleration
		/// </summary>
		[Tooltip("Value is the time is takes accelerate to max speed")]
		[SerializeField]
		private CurveEvaluator acceleration;
		
		/// <summary>
		/// Jump speed 
		/// </summary>
		public float jumpSpeed = 0.3f;

		/// <summary>
		/// Unity events for entering/exiting state
		/// </summary>
		public UnityEvent enterState, exitState;

		/// <summary>
		/// Gets the maximum speed
		/// </summary>
		public float maximumSpeed
		{
			get { return maxSpeed; }
		}

		/// <summary>
		/// Gets the curve
		/// </summary>
		public CurveEvaluator accelerationCurve
		{
			get { return acceleration; }
		}

		/// <summary>
		/// Can the first person character jump in this state
		/// </summary>
		public bool canJump
		{
			get { return jumpSpeed > Mathf.Epsilon; }
		}

		/// <summary>
		/// Plays the exit state
		/// </summary>
		public void ExitState()
		{
			SafelyPlayUnityEvent(exitState);
		}
		
		/// <summary>
		/// Plays the enter state
		/// </summary>
		public void EnterState()
		{
			SafelyPlayUnityEvent(enterState);
		}

		/// <summary>
		/// Helper to safely play the unity event
		/// </summary>
		/// <param name="unityEvent"></param>
		void SafelyPlayUnityEvent(UnityEvent unityEvent)
		{
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
	}
}
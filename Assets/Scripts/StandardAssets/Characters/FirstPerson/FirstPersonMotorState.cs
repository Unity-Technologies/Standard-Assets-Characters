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
	public class FirstPersonMotorState
	{
		/// <summary>
		/// The maximum movement speed
		/// </summary>
		public float maxSpeed = 5f;

		/// <summary>
		/// The curve evaluator for acceleration
		/// </summary>
		[Tooltip("Value is the time is takes accelerate to max speed")]
		public CurveEvaluator acceleration;

		/// <summary>
		/// The curve evaluator for deceleration
		/// </summary>
		[Tooltip("Value is the time is takes decelerate to stationary")]
		public CurveEvaluator deceleration;
		
		/// <summary>
		/// Jump speed 
		/// </summary>
		public float jumpSpeed = 0.3f;

		/// <summary>
		/// Unity events for entering/exiting state
		/// </summary>
		public UnityEvent enterState, exitState;

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
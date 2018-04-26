using System;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace StandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class FirstPersonMotorState
	{
		public float maxSpeed = 5f;

		[Tooltip("Value is the time is takes accelerate to max speed")]
		public CurveEvaluator acceleration;

		[Tooltip("Value is the time is takes decelerate to stationary")]
		public CurveEvaluator deceleration;

		public UnityEvent enterState, exitState;

		public void ExitState()
		{
			SafelyPlayUnityEvent(exitState);
		}
		
		public void EnterState()
		{
			SafelyPlayUnityEvent(enterState);
		}

		private void SafelyPlayUnityEvent(UnityEvent unityEvent)
		{
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
	}
}
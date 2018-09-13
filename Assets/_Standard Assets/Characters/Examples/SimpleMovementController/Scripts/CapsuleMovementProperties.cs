using System;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	[CreateAssetMenu(fileName = "Capsule Movement Properties",
		menuName = "Standard Assets/Characters/Create Capsule Movement", order = 1)]
	public class CapsuleMovementProperties : ScriptableObject
	{
		/// <summary>
		/// The state name
		/// </summary>
		[SerializeField]
		protected string id;

		public string stateId
		{
			get { return id; }
		}

		/// <summary>
		/// The maximum movement speed
		/// </summary>
		[SerializeField]
		protected float maxSpeed = 5f;

		/// <summary>
		/// The curve evaluator for acceleration
		/// </summary>
		[Tooltip("Value is the time is takes accelerate to max speed")]
		[SerializeField]
		protected CurveEvaluator acceleration;

		/// <summary>
		/// Jump speed 
		/// </summary>
		[SerializeField]
		protected float jumpSpeed = 0.3f;
		
		/// <summary>
		/// The length of a stride
		/// </summary>
		[SerializeField, Tooltip("Distance that is considered a stride")]
		protected float strideLength;

		/// <summary>
		/// Gets the stride length
		/// </summary>
		public float strideLengthDistance
		{
			get { return strideLength; }
		}

		public Action<string> enterState, exitState;

		/// <summary>
		/// Gets the maximum speed
		/// </summary>
		public float maximumSpeed
		{
			get { return maxSpeed; }
			set { maxSpeed = value; }
		}

		/// <summary>
		/// Gets the curve
		/// </summary>
		public CurveEvaluator accelerationCurve
		{
			get { return acceleration; }
		}

		/// <summary>
		/// Gets the jump speed
		/// </summary>
		public float jumpingSpeed
		{
			get { return jumpSpeed; }
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
			SafelyBroadcastEvent(exitState);
		}

		/// <summary>
		/// Plays the enter state
		/// </summary>
		public void EnterState()
		{
			SafelyBroadcastEvent(enterState);
		}

		/// <summary>
		/// Helper to safely broadcast the action
		/// </summary>
		void SafelyBroadcastEvent(Action<string> action)
		{
			if (action != null)
			{
				action(id);
			}
		}
	}
}
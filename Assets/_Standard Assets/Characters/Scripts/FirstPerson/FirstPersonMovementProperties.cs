using System;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace StandardAssets.Characters.FirstPerson
{
	/// <summary>
	/// The state that modifies the behaviour of the first person motor
	/// e.g. the difference between standing and crouching
	/// </summary>
	[CreateAssetMenu(fileName = "First Person Movement Properties",
		menuName = "Standard Assets/Characters/Create First Person Movement", order = 1)]
	public class FirstPersonMovementProperties : ScriptableObject
	{
		/// <summary>
		/// Actions that are fired on state change
		/// </summary>
		public Action<string> enterState, exitState;
		
		/// <summary>
		/// The unique identifier - corresponds to a state in the animator
		/// </summary>
		[SerializeField, Tooltip("The unique identifier - corresponds to a state in the animator")]
		protected string id;

		/// <summary>
		/// The maximum movement speed
		/// </summary>
		[SerializeField, Tooltip("The maximum movement speed of the character")]
		protected float maxSpeed = 5f;

		/// <summary>
		/// The curve evaluator for acceleration
		/// </summary>
		[SerializeField, Tooltip("Value is the time is takes accelerate to max speed")]
		protected CurveEvaluator acceleration;

		/// <summary>
		/// The initial Y velocity of a Jump
		/// </summary>
		[SerializeField, Tooltip("The initial Y velocity of a Jump")]
		protected float jumpSpeed = 5f;

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
		private void SafelyBroadcastEvent(Action<string> action)
		{
			if (action != null)
			{
				action(id);
			}
		}

#if UNITY_EDITOR
		private const float k_MinimumMaxSpeed = 0.001f;
		
		// ensure that maxSpeed is above 0.
		private void OnValidate()
		{
			if (maxSpeed <= 0)
			{
				maxSpeed = k_MinimumMaxSpeed;
			}
		}
#endif
	}
}
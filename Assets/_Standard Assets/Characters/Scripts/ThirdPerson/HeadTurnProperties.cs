using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// A serializable class used to store configuration settings for the head turing/look at.
	/// </summary>
	[Serializable]
	public class HeadTurnProperties
	{
		[SerializeField, Tooltip("The animator head look at weight.")]
		protected float headLookAtWeight = 1f;

		[SerializeField, Tooltip("The max angle the head can rotate.")]
		protected float headLookAtMaxRotation = 75f;

		[SerializeField, Tooltip("The speed at which head can rotate.")]
		protected float headLookAtRotationSpeed = 15f;

		[SerializeField, Tooltip("Should head rotation take place while aerial?")]
		protected bool adjustHeadLookAtWhileAerial = true;
		
		[SerializeField, Tooltip("Should head rotation take place during rapid turnarounds?")]
		protected bool adjustHeadLookAtDuringTurnaround = true;
		
		/// <summary>
		/// Gets the look at weight used by the animator.
		/// </summary>
		public float lookAtWeight
		{
			get { return headLookAtWeight; }
		}

		/// <summary>
		/// Gets the max look at rotation.
		/// </summary>
		public float lookAtMaxRotation
		{
			get { return headLookAtMaxRotation; }
		}

		/// <summary>
		/// Gets the rotation look at speed.
		/// </summary>
		public float lookAtRotationSpeed
		{
			get { return headLookAtRotationSpeed; }
		}

		/// <summary>
		/// Gets whether the head look at should be applied while aerial.
		/// </summary>
		public bool lookAtWhileAerial
		{
			get { return adjustHeadLookAtWhileAerial; }
		}

		/// <summary>
		/// Gets whether the head look at should be applied during a turnaround.
		/// </summary>
		public bool lookAtWhileTurnaround
		{
			get { return adjustHeadLookAtDuringTurnaround; }
		}
	}
}
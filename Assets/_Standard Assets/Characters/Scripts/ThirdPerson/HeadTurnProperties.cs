using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class HeadTurnProperties
	{
		[SerializeField]
		protected float headLookAtWeight = 1f;

		[SerializeField]
		protected float headLookAtMaxRotation = 75f;

		[SerializeField]
		protected float headLookAtRotationSpeed = 15f;

		[SerializeField]
		protected bool adjustHeadLookAtWhileAerial = true;
		
		[SerializeField]
		protected bool adjustHeadLookAtDuringTurnaround = true;
		
		public float lookAtWeight
		{
			get { return headLookAtWeight; }
		}

		public float lookAtMaxRotation
		{
			get { return headLookAtMaxRotation; }
		}

		public float lookAtRotationSpeed
		{
			get { return headLookAtRotationSpeed; }
		}

		public bool lookAtWhileAerial
		{
			get { return adjustHeadLookAtWhileAerial; }
		}

		public bool lookAtWhileTurnaround
		{
			get { return adjustHeadLookAtDuringTurnaround; }
		}
	}
}
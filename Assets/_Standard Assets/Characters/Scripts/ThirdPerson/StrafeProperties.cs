using System;
using Attributes;
using Attributes.Types;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class StrafeProperties
	{
		[HelperBox(HelperType.Info,
			"Strafing speeds are specified in terms of normalized speeds. This is because root motion is used to drive actual speeds. The following parameters allow tweaking so that root movement feels natural. i.e. if you have a run forward animation with a speed of 10 but the strafe run only has a speed of 9 then you could set the strafeRunForwardSpeed = 0.9 so that movement is consistent.")]
		[SerializeField]
		protected int strafeInputSamples = 1;
		[SerializeField, Range(0f,1f)]
		protected float strafeForwardSpeed = 1f; 
		[SerializeField, Range(0f,1f)]
		protected float strafeBackwardSpeed = 1f; 
		[SerializeField, Range(0f,1f)]
		protected float strafeLateralSpeed = 1f;
		
		public float normalizedForwardStrafeSpeed
		{
			get { return strafeForwardSpeed; }
		}

		public float normalizedBackwardStrafeSpeed
		{
			get { return strafeBackwardSpeed; }
		}

		public float normalizedLateralStrafeSpeed
		{
			get { return strafeLateralSpeed; }
		}

		public int strafeInputWindowSize
		{
			get { return strafeInputSamples; }
		}
	}
}
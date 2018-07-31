using System;
using Attributes;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class StrafeProperties
	{
		[SerializeField, Range(0f,1f)]
		protected float strafeForwardSpeed = 1f; 
		[SerializeField, Range(0f,1f)]
		protected float strafeBackwardSpeed = 1f; 
		[SerializeField, Range(0f,1f)]
		protected float strafeLateralSpeed = 1f;
		[SerializeField]
		protected int strafeInputSamples = 1;
		
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
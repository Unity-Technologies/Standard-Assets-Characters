using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class AnimationFloatParameter
	{
		[SerializeField]
		protected string parameterName;


		[SerializeField]
		protected float interpolationTime;
		
		public string parameter
		{
			get { return parameterName; }
		}

		public float interpolation
		{
			get { return interpolationTime; }
		}

		public AnimationFloatParameter(string newParameterName, float newInterpolationTime)
		{
			parameterName = newParameterName;
			interpolationTime = newInterpolationTime;
		}
	}
}
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
		protected float minInterpolationTime;

		[SerializeField]
		protected float maxInterpolationTime;
		
		public string parameter
		{
			get { return parameterName; }
		}

		public AnimationFloatParameter(string newParameterName, float newMinInterpolationTime, float newMaxInterpolationTime)
		{
			parameterName = newParameterName;
			minInterpolationTime = newMinInterpolationTime;
			maxInterpolationTime = newMaxInterpolationTime;
		}

		public float GetInterpolationTime(float oldValue, float newValue)
		{
			float valueDifference = Mathf.Clamp(Mathf.Abs(oldValue - newValue), 0, 1);
			float interpolationDifference = maxInterpolationTime - minInterpolationTime;
			return maxInterpolationTime - (valueDifference * interpolationDifference);
		}
	}
}
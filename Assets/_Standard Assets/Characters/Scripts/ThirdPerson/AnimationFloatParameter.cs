using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// A class used to contain a animator float parameter name and corresponding interpolation times used when setting
	/// the parameter.
	/// </summary>
	[Serializable]
	public class AnimationFloatParameter
	{
		[SerializeField, Tooltip("The animator float parameter.")]
		protected string parameterName;

		[SerializeField, Tooltip("The least amount the value will be interpolated by.")]
		protected float minInterpolationTime;

		[SerializeField, Tooltip("The greatest amount the value will be interpolated by.")]
		protected float maxInterpolationTime;
		
		/// <summary>
		/// Gets the parameter name.
		/// </summary>
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

		/// <summary>
		/// Gets the interpolation time using the min and max.
		/// </summary>
		/// <param name="oldValue">The current value.</param>
		/// <param name="newValue">The new value to approach.</param>
		/// <returns>The interpolated value.</returns>
		public float GetInterpolationTime(float oldValue, float newValue)
		{
			float valueDifference = Mathf.Clamp(Mathf.Abs(oldValue - newValue), 0.0f, 1.0f);
			float interpolationDifference = maxInterpolationTime - minInterpolationTime;
			return maxInterpolationTime - (valueDifference * interpolationDifference);
		}
	}
}
using System;
using UnityEngine;

namespace Util
{
	/// <summary>
	/// Struct for represent a set of min and max values
	/// </summary>
	[Serializable]
	public struct FloatRange
	{
		public float minValue;
		public float maxValue;

		/// <summary>
		/// Gets an interpolation time using <see cref="minValue"/> and <see cref="maxValue"/>.
		/// </summary>
		/// <param name="oldValue">The current value.</param>
		/// <param name="newValue">The new value to approach.</param>
		/// <returns>The interpolated value.</returns>
		public float GetInterpolationTime(float oldValue, float newValue)
		{
			float valueDifference = Mathf.Clamp(Mathf.Abs(oldValue - newValue), 0.0f, 1.0f);
			float interpolationDifference = maxValue - minValue;
			return maxValue - (valueDifference * interpolationDifference);
		}
	}
}
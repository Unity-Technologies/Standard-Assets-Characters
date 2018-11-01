using System;
using UnityEngine;

namespace StandardAssets.Characters.Helpers
{
	/// <summary>
	/// Struct for represent a set of min and max values
	/// </summary>
	[Serializable]
	public struct FloatRange
	{
		public float minValue;
		public float maxValue;

		public FloatRange(float minValue, float maxValue)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
		}
		
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
	

	/// Attribute for set minimum and maximum values on <see cref="FloatRange"/>
	/// </summary>
	public class FloatRangeSetupAttribute : Attribute
	{
		public float min { get; private set; }
		public float max { get; private set; }
		public int decimalPoints { get; private set; }
		
		public FloatRangeSetupAttribute(float minToUse, float maxToUse, int decimalPointsToUse = 2)
		{
			min = minToUse;
			max = maxToUse;
			decimalPoints = decimalPointsToUse;
		}
	}	
}
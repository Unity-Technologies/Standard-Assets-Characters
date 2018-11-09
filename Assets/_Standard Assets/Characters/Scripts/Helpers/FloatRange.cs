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
		[SerializeField]
		float m_MinValue;
		[SerializeField]
		float m_MaxValue;

		public FloatRange(float minValue, float maxValue)
		{
			m_MinValue = minValue;
			m_MaxValue = maxValue;
		}
		
		/// <summary>
		/// Gets an interpolation time using <see cref="m_MinValue"/> and <see cref="m_MaxValue"/>.
		/// </summary>
		/// <param name="oldValue">The current value.</param>
		/// <param name="newValue">The new value to approach.</param>
		/// <returns>The interpolated value.</returns>
		public float GetInterpolationTime(float oldValue, float newValue)
		{
			var valueDifference = Mathf.Clamp(Mathf.Abs(oldValue - newValue), 0.0f, 1.0f);
			var interpolationDifference = m_MaxValue - m_MinValue;
			return m_MaxValue - (valueDifference * interpolationDifference);
		}
	}
	
	/// <summary>
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
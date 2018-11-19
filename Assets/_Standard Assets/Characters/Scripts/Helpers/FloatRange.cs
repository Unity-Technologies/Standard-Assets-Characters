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
		[SerializeField, Tooltip("Minimum value in range")]
		float m_MinValue;
		
		[SerializeField, Tooltip("Maximum value in range")]
		float m_MaxValue;


		/// <summary>
		/// Sets up the float range
		/// </summary>
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
		/// <summary>
		/// Minimum value in attribute
		/// </summary>
		public float min { get; private set; }

		/// <summary>
		/// Maximum value in attribute
		/// </summary>
		public float max { get; private set; }

		/// <summary>
		/// Number of decimal points to show in inspector
		/// </summary>
		public int decimalPoints { get; private set; }

		
		/// <summary>
		/// Sets the min, max and number of decimal points
		/// </summary>
		public FloatRangeSetupAttribute(float minToUse, float maxToUse, int decimalPointsToUse = 2)
		{
			min = minToUse;
			max = maxToUse;
			decimalPoints = decimalPointsToUse;
		}
	}	
}
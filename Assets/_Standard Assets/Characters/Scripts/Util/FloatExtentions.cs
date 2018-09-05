using UnityEngine;

namespace Util
{
	/// <summary>
	/// Float extensions.
	/// </summary>
	public static class FloatExtentions
	{
		/// <summary>
		/// Is floatA equal to floatB? Compares two floats and takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="floatA"></param>
		/// <param name="floatB"></param>
		/// <returns></returns>
		public static bool IsEqualTo(this float floatA, float floatB)
		{
			return Mathf.Abs(floatA - floatB) < Mathf.Epsilon;
		}

		/// <summary>
		/// Is floatA not equal to floatB? Compares two floats and takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="floatA"></param>
		/// <param name="floatB"></param>
		/// <returns></returns>
		public static bool NotEqualTo(this float floatA, float floatB)
		{
			return Mathf.Abs(floatA - floatB) > Mathf.Epsilon;
		}
		
		/// <summary>
		/// Is floatA equal to zero? Takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="floatA"></param>
		/// <returns></returns>
		public static bool IsEqualToZero(this float floatA)
		{
			return Mathf.Abs(floatA) < Mathf.Epsilon;
		}
		
		/// <summary>
		/// Is floatA not equal to zero? Takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="floatA"></param>
		/// <returns></returns>
		public static bool NotEqualToZero(this float floatA)
		{
			return Mathf.Abs(floatA) > Mathf.Epsilon;
		}
		
		/// <summary>
		/// Remaps the value by the given ranges.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="currentMin">The current range's lower bound</param>
		/// <param name="currentMax">The current range's upper bound</param>
		/// <param name="newMin">The new range's lower bound</param>
		/// <param name="newMax">The new range's upper bound</param>
		/// <returns>The remapped value</returns>
		public static float Remap (this float value, float currentMin, float currentMax, float newMin, float newMax) 
		{
			return Mathf.Clamp((value - currentMin) / (currentMax - currentMin) * (newMax - newMin) + newMin, 
							   newMin, newMax);
		}
		
		/// <summary>
		/// Remaps the value by the current range to a 0-1 range.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="currentMin">The current range's lower bound</param>
		/// <param name="currentMax">The current range's upper bound</param>
		/// <returns>The remapped value</returns>
		public static float Remap01 (this float value, float currentMin, float currentMax) 
		{
			return value.Remap(currentMin, currentMax, 0.0f, 1.0f);
		}
	}
}
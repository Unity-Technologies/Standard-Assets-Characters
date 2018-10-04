using UnityEngine;

namespace Util
{
	/// <summary>
	/// Float extensions.
	/// </summary>
	public static class FloatExtensions
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
		/// <returns>The remapped value clamped within <paramref name="newMin"/> and <paramref name="newMax"/></returns>
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
		/// <returns>The remapped value clamped within 0 and 1</returns>
		public static float Remap01 (this float value, float currentMin, float currentMax) 
		{
			return value.Remap(currentMin, currentMax, 0.0f, 1.0f);
		}
		
		/// <summary>
		/// Wraps a float between -180 and 180.
		/// </summary>
		/// <param name="toWrap">The float to wrap.</param>
		/// <returns>A value between -180 and 180.</returns>
		public static float Wrap180(this float toWrap)
		{
			while (toWrap < -180)
			{
				toWrap += 360;
			}
			
			while (toWrap > 180)
			{
				toWrap -= 360;
			}

			return toWrap;
		}

		/// <summary>
		/// Wraps a float between 0 and 360.
		/// </summary>
		/// <param name="toWrap">The float to wrap.</param>
		/// <returns>A value between 0 and 360.</returns>
		public static float Wrap360(this float toWrap)
		{
			return WrapX(toWrap, 360);
		}

		/// <summary>
		/// Wraps a float between 0 and 1.
		/// </summary>
		/// <param name="toWrap">The float to wrap.</param>
		/// <returns>A value between 0 and 1.</returns>
		public static float Wrap1(this float toWrap)
		{
			return WrapX(toWrap, 1);
		}

		/// <summary>
		/// Wraps a float between 0 and <paramref name="x"/>.
		/// </summary>
		/// <param name="toWrap">The float to wrap.</param>
		/// <param name="x">The max value of the wrap range.</param>
		/// <returns>A value between 0 and <paramref name="x"/>.</returns>
		public static float WrapX(this float toWrap, float x)
		{
			while (toWrap < 0)
			{
				toWrap += x;
			}

			while (toWrap > x)
			{
				toWrap -= x;
			}

			return toWrap;
		}

		/// <summary>
		/// Gets the fraction portion of a float.
		/// </summary>
		/// <param name="number">The float.</param>
		/// <returns>The fraction portion of a float.</returns>
		public static float GetFraction(this float number)
		{
			return number - Mathf.Floor(number);
		}
	}
}
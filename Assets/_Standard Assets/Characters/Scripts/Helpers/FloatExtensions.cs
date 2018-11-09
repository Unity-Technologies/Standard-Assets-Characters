using UnityEngine;

namespace StandardAssets.Characters.Helpers
{
	/// <summary>
	/// Float extensions.
	/// </summary>
	public static class FloatExtensions
	{
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
		/// Wraps a float between 0 and 1.
		/// </summary>
		/// <param name="toWrap">The float to wrap.</param>
		/// <returns>A value between 0 and 1.</returns>
		public static float Wrap1(this float toWrap)
		{
			while (toWrap < 0)
			{
				toWrap += 1.0f;
			}

			while (toWrap > 1.0f)
			{
				toWrap -= 1.0f;
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
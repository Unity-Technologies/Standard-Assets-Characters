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
	}
}
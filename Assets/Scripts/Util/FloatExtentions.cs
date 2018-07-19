using UnityEngine;

namespace Util
{
	/// <summary>
	/// Float extensions.
	/// </summary>
	public static class FloatExtentions
	{
		/// <summary>
		/// Is a equal to b? Compares two floats and takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool IsEqualTo(this float a, float b)
		{
			return Mathf.Abs(a - b) < Mathf.Epsilon;
		}

		/// <summary>
		/// Is a not equal to b? Compares two floats and takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool NotEqualTo(this float a, float b)
		{
			return Mathf.Abs(a - b) > Mathf.Epsilon;
		}
		
		/// <summary>
		/// Is a equal to zero? Compares two floats and takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsEqualToZero(this float a)
		{
			return Mathf.Abs(a) < Mathf.Epsilon;
		}
		
		/// <summary>
		/// Is a not equal to zero? Compares two floats and takes floating point inaccuracy into account, by using Epsilon.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool NotEqualToZero(this float a)
		{
			return Mathf.Abs(a) > Mathf.Epsilon;
		}
	}
}
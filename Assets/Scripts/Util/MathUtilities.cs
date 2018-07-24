using UnityEngine;

namespace Util
{
	public static class MathUtilities
	{
		public static float Wrap180(float toWrap)
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

		public static float Wrap1(float toWrap)
		{
			return WrapX(toWrap, 1);
		}

		public static float WrapX(float toWrap, float x)
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

		public static float GetFraction(float number)
		{
			return number - Mathf.Floor(number);
		}
	}
}
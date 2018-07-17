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
			while (toWrap < 0)
			{
				toWrap += 1;
			}

			while (toWrap > 1)
			{
				toWrap -= 1;
			}

			return toWrap;
		}

		public static float GetFraction(float number)
		{
			return number - Mathf.Floor(number);
		}
	}
}
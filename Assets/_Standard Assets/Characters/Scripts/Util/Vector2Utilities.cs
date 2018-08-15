using UnityEngine;

namespace Util
{
	public static class Vector2Utilities
	{
		public static float Angle(Vector2 from, Vector2 to)
		{
			float angle = Vector2.Angle(from, to);
			Vector3 cross = Vector3.Cross(from, to);

			if (cross.z > 0)
			{
				angle = 360 - angle;
			}
 
			return angle;
		}
	}
}
using UnityEngine;

namespace Util
{
	public static class Vector3Extensions
	{
		public static float GetMagnitudeOnAxis(this Vector3 vector, Vector3 axis)
		{
			float dot = Vector3.Dot(axis, vector.normalized);
			float val = dot * vector.magnitude;
			
			return val;
		}
	}
}
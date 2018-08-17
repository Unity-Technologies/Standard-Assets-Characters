using UnityEngine;

namespace Util
{
	public static class Vector3Extensions
	{
		public static float GetMagnitudeOnAxis(this Vector3 vector, Vector3 axis)
		{
			float vectorMagnitude = vector.magnitude;
			if (vectorMagnitude <= 0)
			{
				return 0;
			}
			float dot = Vector3.Dot(axis, vector / vectorMagnitude);
			float val = dot * vectorMagnitude;
			return val;
		}
		
		/// <summary>
		/// Get the square magnitude from vectorA to vectorB.
		/// </summary>
		/// <returns>The sqr magnitude.</returns>
		/// <param name="vectorA">First vector.</param>
		/// <param name="vectorB">Second vector.</param>
		public static float SqrMagnitudeFrom(this Vector3 vectorA, Vector3 vectorB)
		{
			Vector3 diff = vectorA - vectorB;
			return diff.sqrMagnitude;
		}
	}
}
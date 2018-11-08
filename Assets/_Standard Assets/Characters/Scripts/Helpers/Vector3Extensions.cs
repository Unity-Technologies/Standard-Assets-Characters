using UnityEngine;

namespace StandardAssets.Characters.Helpers
{
	public static class Vector3Extensions
	{
		/// <summary>
		/// Gets the magnitude on an axis given a <see cref="Vector3"/>.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="axis">The axis on which to calculate the magnitude.</param>
		/// <returns>The magnitude.</returns>
		public static float GetMagnitudeOnAxis(this Vector3 vector, Vector3 axis)
		{
			var vectorMagnitude = vector.magnitude;
			if (vectorMagnitude <= 0)
			{
				return 0;
			}
			var dot = Vector3.Dot(axis, vector / vectorMagnitude);
			var val = dot * vectorMagnitude;
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
			var diff = vectorA - vectorB;
			return diff.sqrMagnitude;
		}
	}
}
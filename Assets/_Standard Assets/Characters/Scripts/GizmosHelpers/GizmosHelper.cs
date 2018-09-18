using UnityEngine;

namespace StandardAssets.GizmosHelpers
{
	public static class GizmosHelper
	{
		/// <summary>
		/// Draw an upright capsule.
		/// </summary>
		/// <param name="topSphereCenter">Top sphere center (world position).</param>
		/// <param name="bottomSphereCenter">Bottom sphere center (world position).</param>
		/// <param name="radius">Radius.</param>
		/// <param name="color">Color.</param>
		public static void DrawCapsule(Vector3 topSphereCenter, Vector3 bottomSphereCenter, float radius, Color color)
		{
			Gizmos.color = color;
			
			// Spheres
			Gizmos.DrawWireSphere(topSphereCenter, radius);
			Gizmos.DrawWireSphere(bottomSphereCenter, radius);

			// 4 lines on the sides
			Gizmos.DrawLine(topSphereCenter + Vector3.right * radius,
			                bottomSphereCenter + Vector3.right * radius);
			Gizmos.DrawLine(topSphereCenter + Vector3.left * radius,
			                bottomSphereCenter + Vector3.left * radius);
			Gizmos.DrawLine(topSphereCenter + Vector3.forward * radius,
			                bottomSphereCenter + Vector3.forward * radius);
			Gizmos.DrawLine(topSphereCenter + Vector3.back * radius,
			                bottomSphereCenter + Vector3.back * radius);
		}
	}
}
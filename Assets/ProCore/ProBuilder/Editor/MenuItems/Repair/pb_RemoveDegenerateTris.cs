using UnityEditor;
using UnityEngine;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for removing degerate triangles.
	 */
	public class pb_RemoveDegenerateTris : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Remove Degenerate Triangles", false, pb_Constant.MENU_REPAIR)]
		public static void MenuRemoveDegenerateTriangles()
		{
			int count = 0;
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				pb.ToMesh();

				int[] rm;
				pb.RemoveDegenerateTriangles(out rm);
				count += rm.Length;

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_EditorUtility.ShowNotification("Removed " + (count/3) + " degenerate triangles.");
		}
	}
}

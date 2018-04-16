using UnityEditor;
using UnityEngine;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for replacing vertex colors on broken objces.
	 */
	public class pb_RepairColors : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild Vertex Colors", false, pb_Constant.MENU_REPAIR)]
		public static void MenuRepairColors()
		{
			int count = 0;
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				if( pb.colors == null || pb.colors.Length != pb.vertexCount )
				{
					pb.ToMesh();
					pb.SetColors(pbUtil.FilledArray<Color>(Color.white, pb.vertexCount));
					pb.Refresh();
					pb.Optimize();
		
					count++;
				}
			}

			pb_EditorUtility.ShowNotification("Rebuilt colors for " + count + " ProBuilder Objects.");
		}
	}
}

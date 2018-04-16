using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for manually regenerating all ProBuilder shared indices caches in selection.
	 */
	public class pb_RebuildSharedIndices : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild Shared Indices Cache", true, pb_Constant.MENU_REPAIR)]
		private static bool VertifyRebuildMeshes()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}
		
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild Shared Indices Cache", false, pb_Constant.MENU_REPAIR)]
		public static void DoRebuildMeshes()
		{	
			RebuildSharedIndices( pbUtil.GetComponents<pb_Object>(Selection.transforms) );
		}

		/**
		 *	\brief Rebuild targets if they can't be refreshed.
		 */
		private static void RebuildSharedIndices(pb_Object[] targets, bool interactive = true)
		{
			for(int i = 0; i < targets.Length; i++)
			{
				if(interactive)
				{
					EditorUtility.DisplayProgressBar(
						"Refreshing ProBuilder Objects",
						"Reshaping pb_Object " + targets[i].id + ".",
						((float)i / targets.Length));
				}

				pb_Object pb = targets[i];
				
				try
				{
	 				pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));					

	 				pb.ToMesh();
	 				pb.Refresh();
	 				pb.Optimize();
			 	}
			 	catch(System.Exception e)
			 	{
			 		Debug.LogError("Failed rebuilding " + pb.name + " shared indices cache.\n" + e.ToString());
			 	}
			}

			if(interactive)
			{
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Rebuild Shared Index Cache", "Successfully rebuilt " + targets.Length + " shared index caches", "Okay");
			}
		}
	}
}

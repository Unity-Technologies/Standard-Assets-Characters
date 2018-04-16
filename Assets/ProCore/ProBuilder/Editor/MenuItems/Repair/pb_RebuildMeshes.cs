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
	 * Menu interface for manually regenerating all ProBuilder mesh references in scene.
	 */
	public class pb_RebuildMeshes : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild ProBuilder Objects", true, pb_Constant.MENU_REPAIR)]
		private static bool VertifyRebuildMeshes()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}
		
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild ProBuilder Objects", false, pb_Constant.MENU_REPAIR)]
		public static void DoRebuildMeshes()
		{	
			StripAndProBuilderize( pbUtil.GetComponents<pb_Object>(Selection.transforms) );
		}

		/**
		 *	\brief Rebuild targets if they can't be refreshed.
		 */
		private static void StripAndProBuilderize(pb_Object[] targets, bool interactive = true)
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
					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
			 	}
			 	catch
			 	{
			 		if(pb.msh != null)
			 			RebuildProBuilderMesh(pb);
			 	}
			}

			if(interactive)
			{
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Rebuild ProBuilder Objects", "Successfully rebuilt " + targets.Length + " ProBuilder Objects", "Okay");
			}
		}

		private static void RebuildProBuilderMesh(pb_Object pb)
		{
			try
			{
			 	GameObject go = pb.gameObject;
	 			pb.dontDestroyMeshOnDelete = true;
	 			Undo.DestroyObjectImmediate(pb);

	 			// don't delete pb_Entity here because it won't
	 			// actually get removed till the next frame, and 
	 			// probuilderize wants to add it if it's missing
	 			// (which it looks like it is from c# side but
	 			// is not)

				pb = Undo.AddComponent<pb_Object>(go);
				pbMeshOps.ResetPbObjectWithMeshFilter(pb, true);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}
			catch(System.Exception e)
			{
				Debug.LogError("Failed rebuilding ProBuilder mesh: " + e.ToString());
			}
		}
	}
}

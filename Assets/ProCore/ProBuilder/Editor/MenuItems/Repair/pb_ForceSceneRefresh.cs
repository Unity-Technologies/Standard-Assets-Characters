using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorCommon;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for manually re-generating all ProBuilder geometry in scene.
	 */
	public class pb_ForceSceneRefresh : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Force Refresh Scene", false, pb_Constant.MENU_REPAIR)]
		public static void MenuForceSceneRefresh()
		{
			ForceRefresh(true);
		}

		/**
		 *	\brief Force refreshes all meshes in scene.
		 */
		private static void ForceRefresh(bool interactive)
		{
			pb_Object[] all = (pb_Object[])GameObject.FindObjectsOfType(typeof(pb_Object));
			for(int i = 0; i < all.Length; i++)
			{
				if(interactive)
				EditorUtility.DisplayProgressBar(
					"Refreshing ProBuilder Objects",
					"Reshaping pb_Object " + all[i].id + ".",
					((float)i / all.Length));

				try
				{
					all[i].ToMesh();
					all[i].Refresh();
					all[i].Optimize();
				}
				catch {}
			}

			if(interactive)
			{
				EditorUtility.ClearProgressBar();
				EditorUtility.DisplayDialog("Refresh ProBuilder Objects", "Successfully refreshed all ProBuilder objects in scene.", "Okay");
			}
		}
	}
}

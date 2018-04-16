using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorCommon;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for applying materials to pb_Object after upgrading from Basic to Advanced.
	 */
	public class pb_UpgradeBasicToAdvanced : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Upgrade Scene to Advanced", false, pb_Constant.MENU_REPAIR + 10)]
		public static void MenuUpgradeSceneAdvanced()
		{
			if( !EditorUtility.DisplayDialog("Upgrade Scene to Advanced", "This utility sets the materials on every ProBuilder object in the scene.  Continue?", "Okay", "Cancel") )
				return;

			DoUpgrade((pb_Object[]) Resources.FindObjectsOfTypeAll(typeof(pb_Object)));

			EditorUtility.DisplayDialog("Upgrade ProBuilder Objects", "Successfully upgraded all ProBuilder objects in scene.\n\nIf any of the objects in the scene were prefabs you'll need to 'Apply' changes.", "Okay");
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Upgrade Selection to Advanced", false, pb_Constant.MENU_REPAIR + 10)]
		public static void MenuUpgradeSelectionAdvanced()
		{
			if( !EditorUtility.DisplayDialog("Upgrade Selection to Advanced", "This utility sets the materials on every selected ProBuilder object.  Continue?", "Okay", "Cancel") )
				return;

			DoUpgrade( Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<pb_Object>()).ToArray() );

			EditorUtility.DisplayDialog("Upgrade ProBuilder Objects", "Successfully upgraded all ProBuilder objects in selection", "Okay");
		}

		private static void DoUpgrade(pb_Object[] all)
		{
			bool interactive = all != null && all.Length > 8;

			for(int i = 0 ; i < all.Length; i++)
			{
				pb_Object pb = all[i];

				if(interactive)
				{
					EditorUtility.DisplayProgressBar(
						"Applying Materials",
						"Setting pb_Object " + all[i].id + ".",
						((float)i / all.Length));
				}

				pb.SetFaceMaterial(pb.faces, pb.gameObject.GetComponent<MeshRenderer>().sharedMaterial);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(interactive)
			{
				EditorUtility.ClearProgressBar();
			}

		}
	}
}

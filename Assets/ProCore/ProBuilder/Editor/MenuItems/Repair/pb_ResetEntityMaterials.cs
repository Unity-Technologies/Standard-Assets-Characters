using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorCommon;
using ProBuilder2.Common;
using System.Linq;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for resetting the materials associated with special entity types.
	 */
	public class pb_ResetEntityMaterials : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Repair Entity Materials", false, pb_Constant.MENU_REPAIR)]
		public static void MenuRefreshMeshReferences()
		{	
			RepairEntityMaterials();
		}

		/**
		 *	\brief Force refreshes all meshes in scene.
		 */
		private static void RepairEntityMaterials()
		{
			IEnumerable all = GameObject.FindObjectsOfType(typeof(pb_Entity))
								.Where(x => ((pb_Entity)x).entityType == EntityType.Collider || ((pb_Entity)x).entityType == EntityType.Trigger);

			Material ColliderMat = pb_Constant.ColliderMaterial;
			Material TriggerMat = pb_Constant.TriggerMaterial;

			if( ColliderMat == null )
			{
				Debug.LogError("ProBuilder cannot find Collider material!  Make sure the Collider material asset is in \"Assets/ProCore/ProBuilder/Resources/Material\" folder.");
				return;
			}

			if( TriggerMat == null )
			{
				Debug.LogError("ProBuilder cannot find Trigger material!  Make sure the Trigger material asset is in \"Assets/ProCore/ProBuilder/Resources/Material\" folder.");
				return;
			}

			foreach(pb_Entity ent in all)
			{
				MeshRenderer mr = ent.transform.GetComponent<MeshRenderer>() ?? ent.gameObject.AddComponent<MeshRenderer>();

				mr.sharedMaterials = new Material[1] { ent.entityType == EntityType.Collider ? ColliderMat : TriggerMat };
			}

			EditorUtility.DisplayDialog("Repair Entity Materials", "Successfully reset special entity materials in scene.", "Okay");
		}
	}
}
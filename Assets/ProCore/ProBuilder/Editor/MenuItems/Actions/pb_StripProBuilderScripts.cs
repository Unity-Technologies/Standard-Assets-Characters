using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	public class pb_StripProBuilderScripts : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Strip All ProBuilder Scripts in Scene")]
		public static void StripAllScenes()
		{

			if(!EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts in the scene.  You will no longer be able to edit these objects.  There is no undo, please exercise caution!\n\nAre you sure you want to do this?", "Okay", "Cancel"))
				return;

			pb_Object[] all = (pb_Object[]) Resources.FindObjectsOfTypeAll(typeof(pb_Object) );

			Strip(all);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Strip ProBuilder Scripts in Selection", true, 0)]
		public static bool VerifyStripSelection()
		{
			return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Strip ProBuilder Scripts in Selection")]
		public static void StripAllSelected()
		{
			if(!EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "This will remove all ProBuilder scripts on the selected objects.  You will no longer be able to edit these objects.  There is no undo, please exercise caution!\n\nAre you sure you want to do this?", "Okay", "Cancel"))
				return;

			foreach(Transform t in Selection.transforms)
			{
				foreach(pb_Object pb in t.GetComponentsInChildren<pb_Object>(true))
					DoStrip(pb);
			}
		}

		public static void Strip(pb_Object[] all)
		{
				for(int i = 0; i < all.Length; i++)
				{
					if( EditorUtility.DisplayCancelableProgressBar(
						"Stripping ProBuilder Scripts",
						"Working over " + all[i].id + ".",
						((float)i / all.Length)) )
						break;

					DoStrip(all[i]);
				}

			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("Strip ProBuilder Scripts", "Successfully stripped out all ProBuilder components.", "Okay");

			if(pb_Editor.instance)
				pb_Editor.instance.UpdateSelection();
		}


		public static void DoStrip(pb_Object pb)
		{
			try
			{
				GameObject go = pb.gameObject;

				Renderer ren = go.GetComponent<Renderer>();

				if(ren != null)
					pb_EditorUtility.SetSelectionRenderState(ren, pb_EditorUtility.GetSelectionRenderState());

				if( PrefabUtility.GetPrefabType(go) == PrefabType.Prefab )
					return;

				pb_EditorUtility.VerifyMesh(pb);

				if(pb.msh == null)
				{
					DestroyImmediate(pb);

					if(go.GetComponent<pb_Entity>())
						DestroyImmediate(go.GetComponent<pb_Entity>());

					return;
				}


				string cachedMeshPath;
				Mesh cachedMesh;

				// if meshes are assets and the mesh cache is valid don't duplicate the mesh to an instance.
				if( pb_PreferencesInternal.GetBool(pb_Constant.pbMeshesAreAssets) && pb_EditorMeshUtility.GetCachedMesh(pb, out cachedMeshPath, out cachedMesh) )
				{
					pb.dontDestroyMeshOnDelete = true;
					DestroyImmediate(pb);
					if(go.GetComponent<pb_Entity>())
						DestroyImmediate(go.GetComponent<pb_Entity>());
				}
				else
				{
					Mesh m = pb_MeshUtility.DeepCopy(pb.msh);

					DestroyImmediate(pb);

					if(go.GetComponent<pb_Entity>())
						DestroyImmediate(go.GetComponent<pb_Entity>());

					go.GetComponent<MeshFilter>().sharedMesh = m;
					if(go.GetComponent<MeshCollider>())
						go.GetComponent<MeshCollider>().sharedMesh = m;
				}
			}
			catch {}
		}
	}
}

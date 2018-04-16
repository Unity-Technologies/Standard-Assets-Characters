/**
 * Provides some additional functionality when the FbxSdk and FbxExporter packages
 * are available in the project.
 */

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
#if PROBUILDER_FBX_ENABLED
using Unity.FbxSdk;
using FbxExporters;
using FbxExporters.Editor;
#endif

namespace ProBuilder2.Common
{
	/*
	* Options when exporting FBX files.
	*/
	public class pb_FbxOptions
	{
		public bool quads;
	}

	[InitializeOnLoad]
	public static class pb_Fbx
	{
		private static bool m_FbxIsLoaded = false;

		public static bool FbxEnabled { get { return m_FbxIsLoaded; } }

#if PROBUILDER_FBX_ENABLED

		private static pb_FbxOptions m_FbxOptions = new pb_FbxOptions() {
			quads = true
		};

		static pb_Fbx()
		{
			TryLoadFbxSupport();

			if(m_FbxIsLoaded)
				PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
		}

		static void TryLoadFbxSupport()
		{
			if(m_FbxIsLoaded)
				return;
			FbxPrefab.OnUpdate += OnFbxUpdate;
			ModelExporter.RegisterMeshCallback<pb_Object>(GetMeshForComponent, true);
			m_FbxOptions.quads = pb_PreferencesInternal.GetBool("Export::m_FbxQuads", true);
			m_FbxIsLoaded = true;
		}

		private static void OnFbxUpdate(FbxPrefab updatedInstance, IEnumerable<GameObject> updatedObjects)
		{
			// System.Text.StringBuilder sb = new System.Text.StringBuilder();
			// sb.AppendLine("OnFbxUpdate:");
			// sb.AppendLine("instance: " + updatedInstance.name + " is asset: " + !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(updatedInstance)));
			// sb.AppendLine("objects:");
			// foreach(GameObject go in updatedObjects)
			// 	sb.AppendLine("\t" + go.name);
			// pb_Log.Debug(sb.ToString());
		}

		private static bool GetMeshForComponent(ModelExporter exporter, pb_Object component, FbxNode fbxNode)
		{
			Mesh mesh = new Mesh();
			Material[] materials = null;
			pb_MeshCompiler.Compile(component, ref mesh, out materials, m_FbxOptions.quads ? MeshTopology.Quads : MeshTopology.Triangles);
			exporter.ExportMesh(mesh, fbxNode, materials);
			UnityEngine.Object.DestroyImmediate(mesh);

			// since probuilder can't handle mesh assets that may be externally reloaded, just strip pb
			// stuff for now.
			pb_Entity entity = component.GetComponent<pb_Entity>();
			component.dontDestroyMeshOnDelete = true;
			UnityEngine.Object.DestroyImmediate(component);
			if(entity != null)
			UnityEngine.Object.DestroyImmediate(entity);

			return true;
		}

		private static void PrefabInstanceUpdated(GameObject go)
		{
			// pb_Log.Debug("instance updated: " + go.name);
		}
#endif
	}
}

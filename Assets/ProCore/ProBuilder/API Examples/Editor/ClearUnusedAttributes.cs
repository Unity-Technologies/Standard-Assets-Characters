/**
 *	This script demonstrates one use case for the pb_EditorUtility.onMeshCompiled delegate.
 *
 *	Whenever ProBuilder compiles a mesh it removes the colors, tangents, and uv attributes.
 */

// Uncomment this line to enable this script.
// #define PROBUILDER_API_EXAMPLE

#if PROBUILDER_API_EXAMPLE

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

[InitializeOnLoad]
public class ClearUnusedAttributes : Editor
{
	/**
	 *	Static constructor is called and subscribes to the OnMeshCompiled delegate.
	 */
	static ClearUnusedAttributes()
	{
		pb_EditorUtility.AddOnMeshCompiledListener(OnMeshCompiled);
	}

	~ClearUnusedAttributes()
	{
		pb_EditorUtility.RemoveOnMeshCompiledListener(OnMeshCompiled);
	}

	/**
	 *	When a ProBuilder object is compiled to UnityEngine.Mesh this is called.
	 */
	static void OnMeshCompiled(pb_Object pb, Mesh mesh)
	{
		mesh.uv = null;
		mesh.colors32 = null;
		mesh.tangents = null;

		// Print out the mesh attributes in a neatly formatted string.
		// Debug.Log( pb_MeshUtility.Print(mesh) );
	}
}

#endif

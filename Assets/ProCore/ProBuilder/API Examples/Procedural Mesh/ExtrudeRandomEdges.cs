#if UNITY_EDITOR || UNITY_STANDALONE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;	// Extrude lives here
using System.Linq;

/**
 * Do a snake-like thing with a quad and some extrudes.
 */
public class ExtrudeRandomEdges : MonoBehaviour
{
	private pb_Object pb;
	pb_Face lastExtrudedFace = null;
	public float distance = 1f;

	/**
	 * Build a starting point (in this case, a quad)
	 */
	void Start()
	{
		pb = pb_ShapeGenerator.PlaneGenerator(1, 1, 0, 0, ProBuilder2.Common.Axis.Up, false);
		pb.SetFaceMaterial(pb.faces, pb_Constant.DefaultMaterial);
		lastExtrudedFace = pb.faces[0];
	}

	void OnGUI()
	{
		if(GUILayout.Button("Extrude Random Edge"))
		{
			ExtrudeEdge();
		}
	}

	void ExtrudeEdge()
	{
		pb_Face sourceFace = lastExtrudedFace;

		// fetch a random perimeter edge connected to the last face extruded
		List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
		IEnumerable<pb_WingedEdge> sourceWings = wings.Where(x => x.face == sourceFace);
		List<pb_Edge> nonManifoldEdges = sourceWings.Where(x => x.opposite == null).Select(y => y.edge.local).ToList();
		int rand = (int) Random.Range(0, nonManifoldEdges.Count);
		pb_Edge sourceEdge = nonManifoldEdges[rand];

		// get the direction this edge should extrude in
		Vector3 dir = ((pb.vertices[sourceEdge.x] + pb.vertices[sourceEdge.y]) * .5f) - sourceFace.distinctIndices.Average(x => pb.vertices[x]);
		dir.Normalize();

		// this will be populated with the extruded edge
		pb_Edge[] extrudedEdges;

		// perform extrusion
		pb.Extrude(new pb_Edge[] { sourceEdge }, 0f, false, true, out extrudedEdges);

		// get the last extruded face
		lastExtrudedFace = pb.faces.Last();

		// not strictly necessary, but makes it easier to handle element selection
		pb.SetSelectedEdges( extrudedEdges );

		// translate the vertices
		pb.TranslateVertices(pb.SelectedTriangles, dir * distance);

		// rebuild mesh with new geometry added by extrude
		pb.ToMesh();

		// rebuild mesh normals, textures, collisions, etc
		pb.Refresh();
	}
}
#endif

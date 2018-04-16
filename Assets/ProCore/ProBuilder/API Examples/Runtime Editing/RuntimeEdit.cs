#if UNITY_STANDALONE || UNITY_EDITOR

using UnityEngine;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.Examples
{

	/**
	 *	\brief This class allows the user to select a single face at a time and move it forwards or backwards.
	 *	More advanced usage of the ProBuilder API should make use of the pb_Object->SelectedFaces list to keep
	 *	track of the selected faces.
	 */
	public class RuntimeEdit : MonoBehaviour
	{
		class pb_Selection
		{
			public pb_Object pb;	///< This is the currently selected ProBuilder object.
			public pb_Face face;	///< Keep a reference to the currently selected face.

			public pb_Selection(pb_Object _pb, pb_Face _face)
			{
				pb = _pb;
				face = _face;
			}

			public bool HasObject()
			{
				return pb != null;
			}

			public bool IsValid()
			{
				return pb != null && face != null;
			}

			public bool Equals(pb_Selection sel)
			{
				if(sel != null && sel.IsValid())
					return (pb == sel.pb && face == sel.face);
				else
					return false;
			}

			public void Destroy()
			{
				if(pb != null)
					GameObject.Destroy(pb.gameObject);
			}

			public override string ToString()
			{
				return "pb_Object: " + pb == null ? "Null" : pb.name +
					"\npb_Face: " + ( (face == null) ? "Null" : face.ToString() );
			}
		}

		pb_Selection currentSelection;
		pb_Selection previousSelection;

		private pb_Object preview;
		public Material previewMaterial;

		/**
		 *	\brief Wake up!
		 */
		void Awake()
		{
			SpawnCube();
		}

		/**
		 *	\brief This is the usual Unity OnGUI method.  We only use it to show a 'Reset' button.
		 */
		void OnGUI()
		{
			// To reset, nuke the pb_Object and build a new one.
			if(GUI.Button(new Rect(5, Screen.height - 25, 80, 20), "Reset"))
			{
				currentSelection.Destroy();
				Destroy(preview.gameObject);
				SpawnCube();
			}
		}

		/**
		 *	\brief Creates a new ProBuilder cube and sets it up with a concave MeshCollider.
		 */
		void SpawnCube()
		{
			// This creates a basic cube with ProBuilder features enabled.  See the ProBuilder.Shape enum to
			// see all possible primitive types.
			pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.one);

			// The runtime component requires that a concave mesh collider be present in order for face selection
			// to work.
			pb.gameObject.AddComponent<MeshCollider>().convex = false;

			// Now set it to the currentSelection
			currentSelection = new pb_Selection(pb, null);
		}

		Vector2 mousePosition_initial = Vector2.zero;
		bool dragging = false;
		public float rotateSpeed = 100f;

		/**
		 *	\brief This is responsible for moving the camera around and not much else.
		 */
		public void LateUpdate()
		{
			if(!currentSelection.HasObject())
				return;

			if(Input.GetMouseButtonDown(1) || (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftAlt)))
			{
				mousePosition_initial = Input.mousePosition;
				dragging = true;
			}

			if(dragging)
			{
				Vector2 delta = (Vector3)mousePosition_initial - (Vector3)Input.mousePosition;
				Vector3 dir = new Vector3(delta.y, delta.x, 0f);

				currentSelection.pb.gameObject.transform.RotateAround(Vector3.zero, dir, rotateSpeed * Time.deltaTime);

				// If there is a currently selected face, update the preview.
				if(currentSelection.IsValid())
					RefreshSelectedFacePreview();
			}

			if(Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(0))
			{
				dragging = false;
			}
		}

		/**
		 *	\brief The 'meat' of the operation.  This listens for a click event, then checks for a positive
		 *	face selection.  If the click has hit a pb_Object, select it.
		 */
		public void Update()
		{
			if(Input.GetMouseButtonUp(0) && !Input.GetKey(KeyCode.LeftAlt)) {

				if(FaceCheck(Input.mousePosition))
				{
					if(currentSelection.IsValid())
					{
						// Check if this face has been previously selected, and if so, move the face.
						// Otherwise, just accept this click as a selection.
						if(!currentSelection.Equals(previousSelection))
						{
							previousSelection = new pb_Selection(currentSelection.pb, currentSelection.face);
							RefreshSelectedFacePreview();
							return;
						}

						Vector3 localNormal = pb_Math.Normal( pbUtil.ValuesWithIndices(currentSelection.pb.vertices, currentSelection.face.distinctIndices) );

						if(Input.GetKey(KeyCode.LeftShift))
							currentSelection.pb.TranslateVertices( currentSelection.face.distinctIndices, localNormal.normalized * -.5f );
						else
							currentSelection.pb.TranslateVertices( currentSelection.face.distinctIndices, localNormal.normalized * .5f );

						// Refresh will update the Collision mesh volume, face UVs as applicatble, and normal information.
						currentSelection.pb.Refresh();

						// this create the selected face preview
						RefreshSelectedFacePreview();
					}
				}
			}
		}

		/**
		 *	\brief This is how we figure out what face is clicked.
		 */
		public bool FaceCheck(Vector3 pos)
		{
			Ray ray = Camera.main.ScreenPointToRay (pos);
			RaycastHit hit;

			if( Physics.Raycast(ray.origin, ray.direction, out hit))
			{
				pb_Object hitpb = hit.transform.gameObject.GetComponent<pb_Object>();

				if(hitpb == null)
					return false;

				Mesh m = hitpb.msh;

				int[] tri = new int[3] {
					m.triangles[hit.triangleIndex * 3 + 0],
					m.triangles[hit.triangleIndex * 3 + 1],
					m.triangles[hit.triangleIndex * 3 + 2]
				};

				currentSelection.pb = hitpb;

				return hitpb.FaceWithTriangle(tri, out currentSelection.face);
			}
			return false;
		}

		void RefreshSelectedFacePreview()
		{
			// Copy the currently selected vertices in world space.
			// World space so that we don't have to apply transforms
			// to match the current selection.
			Vector3[] verts = currentSelection.pb.VerticesInWorldSpace(currentSelection.face.indices);

			// face.indices == triangles, so wind the face to match
			int[] indices = new int[verts.Length];
			for(int i = 0; i < indices.Length; i++)
				indices[i] = i;

			// Now go through and move the verts we just grabbed out about .1m from the original face.
			Vector3 normal = pb_Math.Normal(verts);

			for(int i = 0; i < verts.Length; i++)
				verts[i] += normal.normalized * .01f;

			if(preview)
				Destroy(preview.gameObject);

			preview = pb_Object.CreateInstanceWithVerticesFaces(verts, new pb_Face[] { new pb_Face(indices) });
			preview.SetFaceMaterial(preview.faces, previewMaterial);
			preview.ToMesh();
			preview.Refresh();
		}
	}
}
#endif

/**
 *	This script demonstrates how to create a new action that can be accessed from the
 *	ProBuilder toolbar.
 *
 *	A new menu item is registered under "Geometry" actions called "Gen. Shadows".
 *
 *	To enable, remove the #if PROBUILDER_API_EXAMPLE and #endif directives.
 */

// Uncomment this line to enable the menu action.
// #define PROBUILDER_API_EXAMPLE

#if PROBUILDER_API_EXAMPLE

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;

/**
 *	When creating your own actions please use your own namespace.
 */
namespace ProBuilder2.Actions
{
	/**
	 *	This class is responsible for loading the pb_MenuAction into the toolbar and menu.
	 */
	[InitializeOnLoad]
	static class RegisterCustomAction
	{
		/**
		 *	Static initializer is called when Unity loads the assembly.
		 */
		static RegisterCustomAction()
		{
			// This registers a new CreateShadowObject menu action with the toolbar.
			pb_EditorToolbarLoader.RegisterMenuItem(InitCustomAction);
		}

		/**
		 *	Helper function to load a new menu action object.
		 */
		static pb_MenuAction InitCustomAction()
		{
			return new CreateShadowObject();
		}

		/**
		 *	Usually you'll want to add a menu item entry for your action.
		 * 	https://docs.unity3d.com/ScriptReference/MenuItem.html
		 */
		[MenuItem("Tools/ProBuilder/Object/Create Shadow Object", true)]
		static bool MenuVerifyDoSomethingWithPbObject()
		{
			// Using pb_EditorToolbarLoader.GetInstance keeps MakeFacesDoubleSided as a singleton.
			CreateShadowObject instance = pb_EditorToolbarLoader.GetInstance<CreateShadowObject>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Create Shadow Object", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoDoSomethingWithPbObject()
		{
			CreateShadowObject instance = pb_EditorToolbarLoader.GetInstance<CreateShadowObject>();

			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}
	}

	/**
	 *	This is the actual action that will be executed.
	 */
	public class CreateShadowObject : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		private GUIContent gc_volumeSize = new GUIContent("Volume Size", "How far the shadow volume extends from the base mesh.  To visualize, imagine the width of walls.\n\nYou can also select the child ShadowVolume object and turn the Shadow Casting Mode to \"One\" or \"Two\" sided to see the resulting mesh.");

		private bool showPreview
		{
			get { return pb_PreferencesInternal.GetBool("pb_shadowVolumePreview", true); }
			set { pb_PreferencesInternal.SetBool("pb_shadowVolumePreview", value); }
		}

		/**
		 *	What to show in the hover tooltip window.  pb_TooltipContent is similar to GUIContent, with the exception that it also
		 *	includes an optional params[] char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
		 */
		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Gen Shadow Obj",
			"Creates a new ProBuilder mesh child with inverted normals that only exists to cast shadows.  Use to create lit interior scenes with shadows from directional lights.\n\nNote that this exists largely as a workaround for real-time shadow light leaks.  Baked shadows do not require this workaround.",
			""	// Some combination of build settings can cause the compiler to not respection optional params in the pb_TooltipContent c'tor?
		);

		/**
		 *	Determines if the action should be enabled or grayed out.
		 */
		public override bool IsEnabled()
		{
			// `selection` is a helper property on pb_MenuAction that returns a pb_Object[] array from the current selection.
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0;
		}

		/**
		 *	Determines if the action should be loaded in the menu (ex, face actions shouldn't be shown when in vertex editing mode).
		 */
		public override bool IsHidden()
		{
			return pb_Editor.instance == null;
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsEnable()
		{
			if( showPreview )
				DoAction();
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Create Shadow Volume Options", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginChangeCheck();
			float volumeSize = pb_PreferencesInternal.GetFloat("pb_CreateShadowObject_volumeSize", .07f);
			volumeSize = EditorGUILayout.Slider(gc_volumeSize, volumeSize, 0.001f, 1f);
			if( EditorGUI.EndChangeCheck() )
				pb_PreferencesInternal.SetFloat("pb_CreateShadowObject_volumeSize", volumeSize);

			#if !UNITY_4_6 && !UNITY_4_7
			EditorGUI.BeginChangeCheck();
			ShadowCastingMode shadowMode = (ShadowCastingMode) pb_PreferencesInternal.GetInt("pb_CreateShadowObject_shadowMode", (int) ShadowCastingMode.ShadowsOnly);
			shadowMode = (ShadowCastingMode) EditorGUILayout.EnumPopup("Shadow Casting Mode", shadowMode);
			if(EditorGUI.EndChangeCheck())
				pb_PreferencesInternal.SetInt("pb_CreateShadowObject_shadowMode", (int) shadowMode);
			#endif

			EditorGUI.BeginChangeCheck();
			ExtrudeMethod extrudeMethod = (ExtrudeMethod) pb_PreferencesInternal.GetInt("pb_CreateShadowObject_extrudeMethod", (int) ExtrudeMethod.FaceNormal);
			extrudeMethod = (ExtrudeMethod) EditorGUILayout.EnumPopup("Extrude Method", extrudeMethod);
			if(EditorGUI.EndChangeCheck())
				pb_PreferencesInternal.SetInt("pb_CreateShadowObject_extrudeMethod", (int) extrudeMethod);

			if(EditorGUI.EndChangeCheck())
				DoAction();

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Create Shadow Volume"))
			{
				DoAction();
				SceneView.RepaintAll();
				pb_MenuOption.CloseAll();
			}
		}

		/**
		 *	Do the thing.  Return a pb_ActionResult indicating the success/failure of action.
		 */
		public override pb_ActionResult DoAction()
		{
			#if !UNITY_4_6 && !UNITY_4_7
			ShadowCastingMode shadowMode = (ShadowCastingMode) pb_PreferencesInternal.GetInt("pb_CreateShadowObject_shadowMode", (int) ShadowCastingMode.ShadowsOnly);
			#endif
			float extrudeDistance = pb_PreferencesInternal.GetFloat("pb_CreateShadowObject_volumeSize", .08f);
			ExtrudeMethod extrudeMethod = (ExtrudeMethod) pb_PreferencesInternal.GetInt("pb_CreateShadowObject_extrudeMethod", (int) ExtrudeMethod.FaceNormal);

			foreach(pb_Object pb in selection)
			{
				pb_Object shadow = GetShadowObject(pb);

				if(shadow == null)
					continue;

				foreach(pb_Face f in shadow.faces) { f.ReverseIndices(); f.manualUV = true; }
				shadow.Extrude(shadow.faces, extrudeMethod, extrudeDistance);
				shadow.ToMesh();
				shadow.Refresh();
				shadow.Optimize();

				#if !UNITY_4_6 && !UNITY_4_7
				MeshRenderer mr = shadow.gameObject.GetComponent<MeshRenderer>();
				mr.shadowCastingMode = shadowMode;
				if(shadowMode == ShadowCastingMode.ShadowsOnly)
					mr.receiveShadows = false;
				#endif

				Collider collider = shadow.GetComponent<Collider>();

				while(collider != null)
				{
					GameObject.DestroyImmediate(collider);
					collider = shadow.GetComponent<Collider>();
				}
			}

			// This is necessary!  Otherwise the pb_Editor will be working with caches from
			// outdated meshes and throw errors.
			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Create Shadow Object");
		}

		private pb_Object GetShadowObject(pb_Object pb)
		{
			if(pb == null || pb.name.Contains("-ShadowVolume"))
				return null;

			for(int i = 0; i < pb.transform.childCount; i++)
			{
				Transform t = pb.transform.GetChild(i);

				if(t.name.Equals(string.Format("{0}-ShadowVolume", pb.name)))
				{
					pb_Object shadow = t.GetComponent<pb_Object>();

					if(shadow != null)
					{
						pbUndo.RecordObject(shadow, "Update Shadow Object");

						pb_Face[] faces = new pb_Face[pb.faces.Length];

						for(int nn = 0; nn < pb.faces.Length; nn++)
							faces[nn] = new pb_Face(pb.faces[nn]);

						shadow.GeometryWithVerticesFaces(pb.vertices, faces);
						return shadow;
					}
				}
			}

			pb_Object new_shadow = pb_Object.InitWithObject(pb);
			new_shadow.name = string.Format("{0}-ShadowVolume", pb.name);
			new_shadow.MakeUnique();
			new_shadow.transform.SetParent(pb.transform, false);
			Undo.RegisterCreatedObjectUndo(new_shadow.gameObject, "Create Shadow Object");
			return new_shadow;
		}
	}
}
#endif


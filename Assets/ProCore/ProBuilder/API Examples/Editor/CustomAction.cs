/**
 *	This script demonstrates how to create a new action that can be accessed from the 
 *	ProBuilder toolbar.
 *
 *	A new menu item is registered under "Geometry" actions called "Make Double-Sided".
 *	
 *	To enable, remove the #if PROBUILDER_API_EXAMPLE and #endif directives.
 */

// Uncomment this line to enable the menu action.
// #define PROBUILDER_API_EXAMPLE

#if PROBUILDER_API_EXAMPLE

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using ProBuilder2.Interface;
using System.Linq;
using System.Collections.Generic;

namespace MySpecialNamespace.Actions
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
			// This registers a new MakeFacesDoubleSided menu action with the toolbar.
			pb_EditorToolbarLoader.RegisterMenuItem(InitCustomAction);
		}

		/**
		 *	Helper function to load a new menu action object.
		 */
		static pb_MenuAction InitCustomAction()
		{
			return new MakeFacesDoubleSided();
		}

		/**
		 *	Usually you'll want to add a menu item entry for your action.
		 * 	https://docs.unity3d.com/ScriptReference/MenuItem.html
		 */
		[MenuItem("Tools/ProBuilder/Geometry/Make Faces Double-Sided", true)]
		static bool MenuVerifyDoSomethingWithPbObject()
		{
			// Using pb_EditorToolbarLoader.GetInstance keeps MakeFacesDoubleSided as a singleton.
			MakeFacesDoubleSided instance = pb_EditorToolbarLoader.GetInstance<MakeFacesDoubleSided>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Make Faces Double-Sided", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoDoSomethingWithPbObject()
		{
			MakeFacesDoubleSided instance = pb_EditorToolbarLoader.GetInstance<MakeFacesDoubleSided>();

			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}
	}

	/**
	 *	This is the actual action that will be executed.
	 */
	public class MakeFacesDoubleSided : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		/**
		 *	What to show in the hover tooltip window.  pb_TooltipContent is similar to GUIContent, with the exception that it also
		 *	includes an optional params[] char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
		 */
		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Double-Sided",
			"Adds another face to the back of the selected faces."
		);

		/**
		 *	Determines if the action should be enabled or grayed out.
		 */
		public override bool IsEnabled()
		{
			// `selection` is a helper property on pb_MenuAction that returns a pb_Object[] array from the current selection.
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedFaceCount > 0);
		}

		/**
		 *	Determines if the action should be loaded in the menu (ex, face actions shouldn't be shown when in vertex editing mode).
		 */
		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Face;
		}

		/**
		 *	Do the thing.  Return a pb_ActionResult indicating the success/failure of action.
		 */
		public override pb_ActionResult DoAction()
		{
			pbUndo.RecordObjects(selection, "Make Double-Sided Faces");

			foreach(pb_Object pb in selection)
			{
				pb_AppendDelete.DuplicateAndFlip(pb, pb.SelectedFaces);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			// This is necessary!  Otherwise the pb_Editor will be working with caches from
			// outdated meshes and throw errors.
			pb_Editor.Refresh();
			
			return new pb_ActionResult(Status.Success, "Make Faces Double-Sided");
		}
	}
}
#endif


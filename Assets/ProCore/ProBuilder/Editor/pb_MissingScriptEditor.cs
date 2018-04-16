/**
 * Repairs missing pb_Object and pb_Entity references.  It is based
 * on this article by Unity Gems: http://unitygems.com/lateral1/
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Extends MonoBehaviour Inspector, automatically fixing missing script
	 * references (typically caused by ProBuilder upgrade process).
	 */
	[CustomEditor(typeof(MonoBehaviour))]
	public class pb_MissingScriptEditor : Editor
	{
	#region Members

		static bool applyDummyScript = true;	///< If true, any null components that can't be set will have this script applied to their reference, allowing us to later remove them.
		static float index = 0;					///< general idea of where we are in terms of processing this scene.
		static float total;						///< general idea of how many missing script references are in this scene.

		static bool doFix = false;				///< while true, the inspector will attempt to cycle to broken gameobjects until none are found.
		static List<GameObject> unfixable = new List<GameObject>();	///< if a non-pb missing reference is encountered, need to let the iterator know not to bother,

		static MonoScript _mono_pb;				///< MonoScript assets
		static MonoScript _mono_pe;				///< MonoScript assets
		static MonoScript _mono_dummy;			///< MonoScript assets

		/**
		 * Load the pb_Object and pb_Entity classes to MonoScript assets.  Saves us from having to fall back on Reflection.
		 */
		static void LoadMonoScript()
		{
			GameObject go = new GameObject();

			pb_Object pb = go.AddComponent<pb_Object>();
			pb_Entity pe = go.GetComponent<pb_Entity>();
			if(pe == null)
				pe = go.AddComponent<pb_Entity>();
			pb_DummyScript du = go.AddComponent<pb_DummyScript>();

			_mono_pb = MonoScript.FromMonoBehaviour( pb );
			_mono_pe = MonoScript.FromMonoBehaviour( pe );
			_mono_dummy = MonoScript.FromMonoBehaviour( du );

			DestroyImmediate(go);
		}

		public MonoScript pb_monoscript
		{
			get
			{
				if(_mono_pb == null) LoadMonoScript();
				return _mono_pb;
			}
		}

		public MonoScript pe_monoscript
		{
			get
			{
				if(_mono_pe == null) LoadMonoScript();
				return _mono_pe;
			}
		}

		public MonoScript dummy_monoscript
		{
			get
			{
				if(_mono_dummy == null) LoadMonoScript();
				return _mono_dummy;
			}
		}
	#endregion

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Repair Missing Script References")]
		public static void MenuRepairMissingScriptReferences()
		{
			FixAllScriptReferencesInScene();
		}

		static void FixAllScriptReferencesInScene()
		{
			EditorApplication.ExecuteMenuItem("Window/Inspector");

			Object[] all = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(x => ((GameObject)x).GetComponents<Component>().Any(n => n == null) ).ToArray();
			total = all.Length;

			unfixable.Clear();

			if(total > 1)
			{
				Undo.RecordObjects(all, "Fix missing script references");

				index = 0;
				doFix = true;

				Next();
			}
			else
			{
				if( applyDummyScript )
					DeleteDummyScripts();

				EditorUtility.DisplayDialog("Success", "No missing ProBuilder script references found.", "Okay");
			}
		}

		/**
		 * Advance to the next gameobject with missing components.  If none are found, display dialog and exit.
		 */
		static void Next()
		{
			bool earlyExit = false;

			if( EditorUtility.DisplayCancelableProgressBar("Repair ProBuilder Script References", "Fixing " + (int)Mathf.Floor(index+1) + " out of " + total + " objects in scene.", ((float)index/total) ) )
			{
				earlyExit = true;
				doFix = false;
			}

			if(!earlyExit)
			{
				// Cycle through FindObjectsOfType on every Next() because using a static list didn't work for some reason.
				foreach(GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
				{
					if(go.GetComponents<Component>().Any(x => x == null) && !unfixable.Contains(go))
					{
						if(	(PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance ||
							 PrefabUtility.GetPrefabType(go) == PrefabType.Prefab ) )
						{
							GameObject pref = (GameObject)PrefabUtility.GetPrefabParent(go);

							if(pref && (pref.GetComponent<pb_Object>() || pref.GetComponent<pb_Entity>()))
							{
								unfixable.Add(go);
								continue;
							}
						}

						if(go.hideFlags != HideFlags.None)
						{
							unfixable.Add(go);
							continue;
						}

						Selection.activeObject = go;

						return;
					}
				}
			}

			pb_Object[] pbs = (pb_Object[])Resources.FindObjectsOfTypeAll(typeof(pb_Object));

			for(int i = 0; i < pbs.Length; i++)
			{
				EditorUtility.DisplayProgressBar("Checking ProBuilder Meshes", "Refresh " + (i+1) + " out of " + total + " objects in scene.", ((float)i/pbs.Length) );

				try
				{
					pbs[i].ToMesh();
					pbs[i].Refresh();
					pbs[i].Optimize();
				} catch (System.Exception e)
				{
					Debug.LogWarning("Failed reconstituting " + pbs[i].name + ".  Proceeding with upgrade anyways.  Usually this means a prefab is already fixed, and just needs to be instantiated to take effect.\n" + e.ToString());
				}
			}

			EditorUtility.ClearProgressBar();

			if( applyDummyScript )
				DeleteDummyScripts();

			EditorUtility.DisplayDialog("Success", "Successfully repaired " + total + " ProBuilder objects.", "Okay");

			if(!pb_EditorSceneUtility.SaveCurrentSceneIfUserWantsTo())
				Debug.LogWarning("Repaired script references will be lost on exit if this scene is not saved!");

			doFix = false;
			skipEvent = true;
		}

		/**
		 * SerializedProperty names found in pb_Entity.
		 */
		List<string> PB_OBJECT_SCRIPT_PROPERTIES = new List<string>()
		{
			"_sharedIndices",
			"_vertices",
			"_uv",
			"_sharedIndicesUV",
			"_quads"
		};

		/**
		 * SerializedProperty names found in pb_Object.
		 */
		List<string> PB_ENTITY_SCRIPT_PROPERTIES = new List<string>()
		{
			"pb",
			"userSetDimensions",
			"_entityType",
			"forceConvex"
		};

		// Prevents ArgumentException after displaying 'Done' dialog.  For some reason the Event loop skips layout phase after DisplayDialog.
		private static bool skipEvent = false;

		public override void OnInspectorGUI()
		{
			if(skipEvent && Event.current.type == EventType.Repaint)
			{
				skipEvent = false;
				return;
			}

			SerializedProperty scriptProperty = this.serializedObject.FindProperty("m_Script");

			if(scriptProperty == null || scriptProperty.objectReferenceValue != null)
			{
				if(doFix)
				{
					if(Event.current.type == EventType.Repaint)
					{
						Next();
					}
				}
				else
				{
					base.OnInspectorGUI();
				}

				return;
			}

			int pbObjectMatches = 0, pbEntityMatches = 0;

			// Shows a detailed tree view of all the properties in this serializedobject.
			// GUILayout.Label( SerializedObjectToString(this.serializedObject) );

			SerializedProperty iterator = this.serializedObject.GetIterator();

			iterator.Next(true);

			while( iterator.Next(true) )
			{
				if( PB_OBJECT_SCRIPT_PROPERTIES.Contains(iterator.name) )
					pbObjectMatches++;

				if( PB_ENTITY_SCRIPT_PROPERTIES.Contains(iterator.name) )
					pbEntityMatches++;
			}

			// If we can fix it, show the help box, otherwise just default inspector it up.
			if(pbObjectMatches >= 3 || pbEntityMatches >= 3)
			{
				EditorGUILayout.HelpBox("Missing Script Reference\n\nProBuilder can automatically fix this missing reference.  To fix all references in the scene, click \"Fix All in Scene\".  To fix just this one, click \"Reconnect\".", MessageType.Warning);
			}
			else
			{
				if(doFix)
				{
					if( applyDummyScript )
					{
						index += .5f;
						scriptProperty.objectReferenceValue = dummy_monoscript;
						scriptProperty.serializedObject.ApplyModifiedProperties();
						scriptProperty = this.serializedObject.FindProperty("m_Script");
						scriptProperty.serializedObject.Update();
					}
					else
					{
						unfixable.Add( ((Component)target).gameObject );
					}

					Next();
					GUIUtility.ExitGUI();
					return;
				}
				else
				{
					base.OnInspectorGUI();
				}

				return;
			}

			GUI.backgroundColor = Color.green;

			if(!doFix)
			{
				if(GUILayout.Button("Fix All in Scene"))
				{
					FixAllScriptReferencesInScene();
					return;
				}
			}

			GUI.backgroundColor = Color.cyan;

			if((doFix && Event.current.type == EventType.Repaint) || GUILayout.Button("Reconnect"))
			{
				if(pbObjectMatches >= 3)	// only increment for pb_Object otherwise the progress bar will fill 2x faster than it should
				{
					index++;
				}
				else
				{
					// Make sure that pb_Object is fixed first if we're automatically cycling objects.
					if(doFix && ((Component)target).gameObject.GetComponent<pb_Object>() == null)
						return;
				}

				if(!doFix)
				{
					Undo.RegisterCompleteObjectUndo(target, "Fix missing reference.");
				}

				// Debug.Log("Fix: " + (pbObjectMatches > 2 ? "pb_Object" : "pb_Entity") + "  " + ((Component)target).gameObject.name);

				scriptProperty.objectReferenceValue = pbObjectMatches >= 3 ? pb_monoscript : pe_monoscript;
				scriptProperty.serializedObject.ApplyModifiedProperties();
				scriptProperty = this.serializedObject.FindProperty("m_Script");
				scriptProperty.serializedObject.Update();

				if(doFix)
					Next();

				GUIUtility.ExitGUI();
			}

			GUI.backgroundColor = Color.white;
		}

		/**
		 * Scan the scene for gameObjects referencing `pb_DummyScript` and delete them.
		 */
		static void DeleteDummyScripts()
		{
			pb_DummyScript[] dummies = (pb_DummyScript[])Resources.FindObjectsOfTypeAll(typeof(pb_DummyScript));
			dummies = dummies.Where(x => x.hideFlags == HideFlags.None).ToArray();

			if(dummies.Length > 0)
			{
				int ret = EditorUtility.DisplayDialogComplex("Found Unrepairable Objects", "Repair script found " + dummies.Length + " missing components that could not be repaired.  Would you like to delete those components now, or attempt to rebuild (ProBuilderize) them?", "Delete", "Cancel", "ProBuilderize");

				switch(ret)
				{
					case 1:	// cancel
					{}
					break;

					default:
					{
						// Delete and ProBuilderize
						if(ret == 2)
						{
							// Only interested in objects that have 2 null components (pb_Object and pb_Entity)
							Object[] broken = (Object[])Resources.FindObjectsOfTypeAll(typeof(GameObject))
								.Where(x => !x.Equals(null) &&
								x is GameObject &&
								((GameObject)x).GetComponents<pb_DummyScript>().Length == 2 &&
								((GameObject)x).GetComponent<MeshRenderer>() != null &&
								((GameObject)x).GetComponent<MeshFilter>() != null &&
								((GameObject)x).GetComponent<MeshFilter>().sharedMesh != null
								).ToArray();

							broken = broken.Distinct().ToArray();

							ProBuilder2.Actions.ProBuilderize.DoProBuilderize(
								System.Array.ConvertAll(broken, x => (GameObject) x)
									.Select(x => x.GetComponent<MeshFilter>()),
								pb_MeshImporter.Settings.Default);
						}

						// Always delete components
						Undo.RecordObjects(dummies.Select(x=>x.gameObject).ToArray(), "Delete Broken Scripts");

						for(int i = 0; i < dummies.Length; i++)
							GameObject.DestroyImmediate( dummies[i] );
					}
					break;
				}

			}
		}

		/**
		 * Returns a formatted string with all properties in serialized object.
		 */
		static string SerializedObjectToString(SerializedObject serializedObject)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			if(serializedObject == null)
			{
				sb.Append("NULL");
				return sb.ToString();
			}

			SerializedProperty iterator = serializedObject.GetIterator();

			iterator.Next(true);


			while( iterator.Next(true) )
			{
				string tabs = "";
				for(int i = 0; i < iterator.depth; i++) tabs += "\t";

				sb.AppendLine(tabs + iterator.name + (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.type.Contains("Component") && iterator.objectReferenceValue == null ? " -> NULL" : "") );

				tabs += "  - ";

				sb.AppendLine(tabs + "Type: (" + iterator.type + " / " + iterator.propertyType + " / " + " / " + iterator.name + ")");
				sb.AppendLine(tabs + iterator.propertyPath);
				sb.AppendLine(tabs + "Value: " + SerializedPropertyValue(iterator));
			}

			return sb.ToString();
		}

		/**
		 * Return a string from the value of a SerializedProperty.
		 */
		static string SerializedPropertyValue(SerializedProperty sp)
		{
			switch(sp.propertyType)
			{
				case SerializedPropertyType.Integer:
					return sp.intValue.ToString();

				case SerializedPropertyType.Boolean:
					return sp.boolValue.ToString();

				case SerializedPropertyType.Float:
					return sp.floatValue.ToString();

				case SerializedPropertyType.String:
					return sp.stringValue.ToString();

				case SerializedPropertyType.Color:
					return sp.colorValue.ToString();

				case SerializedPropertyType.ObjectReference:
					return (sp.objectReferenceValue == null ? "null" : sp.objectReferenceValue.name);

				case SerializedPropertyType.LayerMask:
					return sp.intValue.ToString();

				case SerializedPropertyType.Enum:
					return sp.enumValueIndex.ToString();

				case SerializedPropertyType.Vector2:
					return sp.vector2Value.ToString();

				case SerializedPropertyType.Vector3:
					return sp.vector3Value.ToString();

				// Not public api as of 4.3?
				// case SerializedPropertyType.Vector4:
				// 	return sp.vector4Value.ToString();

				case SerializedPropertyType.Rect:
					return sp.rectValue.ToString();

				case SerializedPropertyType.ArraySize:
					return sp.intValue.ToString();

				case SerializedPropertyType.Character:
					return "Character";

				case SerializedPropertyType.AnimationCurve:
					return sp.animationCurveValue.ToString();

				case SerializedPropertyType.Bounds:
					return sp.boundsValue.ToString();

				case SerializedPropertyType.Gradient:
					return "Gradient";

				default:
					return "Unknown type";
			}
		}

	}
}

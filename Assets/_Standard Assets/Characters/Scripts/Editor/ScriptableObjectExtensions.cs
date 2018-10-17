// Developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// <see cref="ScriptableObject"/> editor extensions.
	/// </summary>
	public static class ScriptableObjectExtensions
	{
		/// <summary>
		/// Draws all values under the object reference. Provides a button to create a new ScriptableObject
		/// if the field is null.
		/// </summary>
		/// <param name="property">The SerializedProperty to draw</param>
		/// <param name="bindingFlags">The flags used to filed the field type.</param>
		public static void DrawExtended(this SerializedProperty property, 
		                                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)
		{
			if (property.objectReferenceValue != null)
			{
				//property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName, true);
				EditorGUILayout.PropertyField(property, GUIContent.none, true);
				if (GUI.changed)
				{
					property.serializedObject.ApplyModifiedProperties();
				}
				if (property.objectReferenceValue == null)
				{
					GUIUtility.ExitGUI();
				}

				if (property.isExpanded)
				{
					// Draw a background that shows us clearly which fields are part of the ScriptableObject

					EditorGUI.indentLevel++;
					var data = (ScriptableObject) property.objectReferenceValue;
					SerializedObject serializedObject = new SerializedObject(data);

					// Iterate over all the values and draw them
					SerializedProperty prop = serializedObject.GetIterator();
					float y = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					if (prop.NextVisible(true))
					{
						do
						{
							// Don't bother drawing the class file
							if (prop.name == "m_Script")
							{
								continue;
							}
							float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
							EditorGUILayout.PropertyField(prop, true);
							y += height + EditorGUIUtility.standardVerticalSpacing;
						} while (prop.NextVisible(false));
					}

					if (GUI.changed)
					{
						serializedObject.ApplyModifiedProperties();
					}

					EditorGUI.indentLevel--;
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.ObjectField(property);
				if (GUILayout.Button("Create"))
				{
					string selectedAssetPath = "Assets";
					var behaviour = property.serializedObject.targetObject as MonoBehaviour;
					if (behaviour != null)
					{
						MonoScript ms = MonoScript.FromMonoBehaviour(behaviour);
						selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
					}

					Type type = property.GetType(bindingFlags);
					if (type == null)
					{
						Debug.LogErrorFormat("Cannot get type of {0}. Consider changing the BindingFlags of the DrawExtended method", property);
						return;
					}
					if (type.IsArray)
					{
						type = type.GetElementType();
					}
					else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
					{
						type = type.GetGenericArguments()[0];
					}
					property.objectReferenceValue = CreateAssetWithSavePrompt(type, selectedAssetPath);
				}
				GUILayout.EndHorizontal();
			}

			property.serializedObject.ApplyModifiedProperties();
		}

		// Creates a new ScriptableObject via the default Save File panel
		private static ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
		{
			string defaultName = string.Format("New {0}.asset", type.Name);
			string message = string.Format("Enter a file name for the {0} ScriptableObject.", type.Name);
			path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", defaultName, "asset", message, path);
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			EditorGUIUtility.PingObject(asset);
			return asset;
		}
		
		private static Type GetType(this SerializedProperty property, BindingFlags bindingFlags)
		{
			Type containingType = property.serializedObject.targetObject.GetType();
			FieldInfo fieldInfo = containingType.GetField(property.propertyPath, bindingFlags);
			return fieldInfo == null ? null : fieldInfo.FieldType;
		}
	}
}
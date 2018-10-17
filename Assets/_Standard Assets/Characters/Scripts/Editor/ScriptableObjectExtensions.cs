// Developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	public static class ScriptableObjectExtensions
	{
		// TODO derive type from property instead of passing it through
		public static void DrawExtended(this SerializedProperty property, Type fieldType)
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
					if (property.serializedObject.targetObject is MonoBehaviour)
					{
						MonoScript ms =
							MonoScript.FromMonoBehaviour((MonoBehaviour) property.serializedObject.targetObject);
						selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
					}

					Type type = fieldType;
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
			path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", "New " + type.Name + ".asset", "asset",
			                                            "Enter a file name for the ScriptableObject.", path);
			if (path == "") return null;
			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			EditorGUIUtility.PingObject(asset);
			return asset;
		}
	}
}
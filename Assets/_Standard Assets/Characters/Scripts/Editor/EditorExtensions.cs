// Based off of work developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// <see cref="SerializedObject"/> extensions.
	/// </summary>
	public static class SerializedObjectExtensions
	{
		/// <summary>
		/// Draws all values under the object reference. Provides a button to create a new ScriptableObject
		/// if the field is null.
		/// </summary>
		/// <param name="serializedObject">The serializedProperty containing the ScriptableObject.</param>
		/// <param name="scriptableObjectName">The field name of the ScriptableObject to draw.</param>
		/// <param name="labelOverride">The title of the field</param>
		/// <param name="bindingFlags">The flags used to find the field's type through reflection.</param>
		public static void DrawExtendedScriptableObject(this SerializedObject serializedObject, string scriptableObjectName, string labelOverride = "",
		                                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)
		{
			string[] properties = scriptableObjectName.Split('.');
			SerializedProperty property = null;
			foreach (string s in properties)
			{
				if (property == null)
				{
					property = serializedObject.FindProperty(s);
				}
				else
				{
					property = property.FindPropertyRelative(s);
				}
			}
			
			if (property == null || property.propertyType != SerializedPropertyType.ObjectReference || 
			    (property.objectReferenceValue != null && !(property.objectReferenceValue is ScriptableObject)))
			{
				Debug.LogErrorFormat(serializedObject.targetObject, "ScriptableObject with name: {0} not found on {1}", 
				                     scriptableObjectName, serializedObject.targetObject);
				return;
			}
			if (property.objectReferenceValue != null)
			{
				GUILayout.BeginHorizontal();
				var title = string.IsNullOrEmpty(labelOverride) ? property.displayName : labelOverride;
				property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, title, true);
				EditorGUILayout.PropertyField(property, GUIContent.none, true);
				GUILayout.EndHorizontal();
				
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
					GUI.Box(EditorGUILayout.BeginVertical(), GUIContent.none);
					UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(property.objectReferenceValue);
					editor.OnInspectorGUI();
					EditorGUILayout.EndVertical();
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

		/// <summary>
		/// Finds and draws the properties given under a foldout and within a GUI.Box if <paramref name="foldout"/> is true.
		/// </summary>
		/// <param name="serializedObject">The serializedProperty with the given fields</param>
		/// <param name="title">The title of the drawn box</param>
		/// <param name="fields">The fields to draw</param>
		/// <param name="foldout">The value of the drawn foldout</param>
		public static void DrawFoldoutBoxedFields(this SerializedObject serializedObject, string title, string[] fields, ref bool foldout)
		{
			GUI.Box(EditorGUILayout.BeginVertical(), GUIContent.none);
			foldout = EditorGUILayout.Foldout(foldout, title);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				foreach (var propertyPath in fields)
				{
					SerializedProperty property = serializedObject.FindProperty(propertyPath);
					if (property != null)
					{
						EditorGUILayout.PropertyField(property, true);
					}
					else
					{
						Debug.LogErrorFormat("Property: {0} not found in {1}", propertyPath, 
						                     serializedObject.targetObject);
					}
				}
				EditorGUI.indentLevel--;
			}
			GUILayout.EndHorizontal();
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		// Creates a new ScriptableObject via the default Save File panel
		static ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
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
	}

	/// <summary>
	/// <see cref="SerializedProperty"/> extensions.
	/// </summary>
	public static class SerializedPropertyExtensions
	{
		/// <summary>
		/// Gets the type of a <see cref="SerializedProperty"/>.
		/// </summary>
		/// <param name="property">The property to query.</param>
		/// <param name="bindingFlags">The flags used to find the field's type through reflection.</param>
		/// <returns></returns>
		public static Type GetType(this SerializedProperty property, BindingFlags bindingFlags)
		{
			Type containingType = property.serializedObject.targetObject.GetType();
			FieldInfo fieldInfo = containingType.GetField(property.propertyPath, bindingFlags);
			return fieldInfo == null ? null : fieldInfo.FieldType;
		}
	}
}
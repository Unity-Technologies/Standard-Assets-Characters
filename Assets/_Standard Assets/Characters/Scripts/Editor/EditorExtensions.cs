// Based off of work developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.Editor
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
		/// <param name="isScriptableObjectFieldEditable">Whether the ScriptableObject property field is editable.</param>
		/// <param name="bindingFlags">The flags used to find the field's type through reflection.</param>
		public static void DrawExtendedScriptableObject(this SerializedObject serializedObject, string scriptableObjectName, string labelOverride = "",
		                                bool isScriptableObjectFieldEditable = true, 
		                                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)
		{
			string[] properties = scriptableObjectName.Split('.');
			SerializedProperty property = null;
			foreach (string s in properties)
			{
				property = property == null ? serializedObject.FindProperty(s) : property.FindPropertyRelative(s);
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
				if (!isScriptableObjectFieldEditable)
				{
					EditorGUI.BeginDisabledGroup(true);
				}
				EditorGUILayout.PropertyField(property, GUIContent.none, true);
				if (!isScriptableObjectFieldEditable)
				{
					EditorGUI.EndDisabledGroup();
				}
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
					EditorGUILayout.BeginVertical();
					UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(property.objectReferenceValue);
					editor.OnInspectorGUI();
					EditorGUILayout.EndVertical();
				}
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
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
				EditorGUILayout.EndHorizontal();
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
		/// <param name="beginning">Action fired at the start of the GUI.Box</param>
		/// <param name="end">Action fired at the end of the GUI.Box</param>
		public static void DrawFieldsUnderFoldout(this SerializedObject serializedObject, string title, string[] fields,
		                                          ref bool foldout, Action beginning = null, Action end = null)
		{
			foldout = EditorGUILayout.Foldout(foldout, title);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				if (beginning != null)
				{
					beginning();
				}
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
				if (end != null)
				{
					end();
				}
				EditorGUI.indentLevel--;
			}
			
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
			var targetObject = GetTargetObjectWithProperty(property);
			if (targetObject == null)
			{
				return null;
			}
			Type type = targetObject.GetType();
			var fieldName = property.propertyPath.Split('.').Last();
			FieldInfo fieldInfo = type.GetField(fieldName, bindingFlags);
			return fieldInfo == null ? null : fieldInfo.FieldType;
		}
		
		// Given a SerializedProperty/path get the object
		static object GetTargetObjectWithProperty(SerializedProperty prop)
		{
			object obj = prop.serializedObject.targetObject;
			var elements = prop.propertyPath.Split('.');
			foreach (var element in elements.Take(elements.Length - 1))
			{
				obj = GetValue(obj, element);
			}
			return obj;
		}

		// Get Field/property information using recursion
		static object GetValue(object source, string name)
		{
			if (source == null)
			{
				return null;
			}
			
			var type = source.GetType();
			while (type != null)
			{
				var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (field != null)
				{
					return field.GetValue(source);
				}
				var property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | 
												      BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (property != null)
				{
					return property.GetValue(source, null);
				}
				type = type.BaseType;
			}
			return null;
		}
	}
}
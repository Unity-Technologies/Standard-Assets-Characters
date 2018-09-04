using System;
using System.Collections.Generic;
using Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Experimental.XR;

namespace StandardAssets.Characters.Attributes.Editor
{
	/// <summary>
	/// Custom property drawer for the <see cref="AnimatorParameterNameAttribute"/>
	/// This will use the given animator to retrieve the parameters and display a drop down.
	/// </summary>
	[CustomPropertyDrawer(typeof(AnimatorParameterNameAttribute))]
	public class AnimatorParameterNamePropertyDrawer : PropertyDrawer
	{
		/// <summary>
		/// Uses the <see cref="Animator"/> found through the <paramref name="property"/> to retrieve a list on
		/// <see cref="AnimatorControllerParameter"/> to display as a drop down.
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var animatorStateParameterAttribute = (AnimatorParameterNameAttribute) attribute;
			SerializedProperty animatorProperty = EditorHelpers.FindPropertyRelative(property, 
												  animatorStateParameterAttribute.animatorField);
			if (animatorProperty == null || animatorProperty.objectReferenceValue == null)
			{
				base.OnGUI(position, property, label);
				return;
			}
			var animator = animatorProperty.objectReferenceValue as AnimatorController;
			if (animator == null)
			{
				base.OnGUI(position, property, label);
				return;
			}

			AnimatorControllerParameter[] parameters = animator.parameters;
			string[] parametersAsStrings = ParametersAsArray(parameters, animatorStateParameterAttribute.type);

			string s = property.stringValue;
			int index = EditorGUI.Popup(position, property.displayName, GetIndex(s, parametersAsStrings), 
										parametersAsStrings);
			property.stringValue = parametersAsStrings[index];
		}

		private string[] ParametersAsArray(AnimatorControllerParameter[] parameters, AnimatorControllerParameterType? type)
		{
			List<string> strings = new List<string>();
			foreach (AnimatorControllerParameter parameter in parameters)
			{
				if (type == null || parameter.type == type)
				{
					strings.Add(parameter.name);
				}
			}
			return strings.ToArray();
		}
		
		private int GetIndex(string selected, IList<string> parameters)
		{
			for (int index = 0; index < parameters.Count; index++)
			{
				string stateName = parameters[index];
				if (stateName == selected)
				{
					return index;
				}
			}
			return 0;
		}
	}
}
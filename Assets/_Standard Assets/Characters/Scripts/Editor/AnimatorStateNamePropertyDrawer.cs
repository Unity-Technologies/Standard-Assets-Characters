using System.Collections.Generic;
using Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace StandardAssets.Characters.Attributes.Editor
{
	/// <summary>
	/// Custom property drawer for the <see cref="AnimatorFloatParameterAttribute"/>.
	/// This will use the given animator to retrieve the float parameters and display a drop down for the parameter
	/// name of a <see cref="StandardAssets.Characters.ThirdPerson.AnimationFloatParameter"/>.
	/// </summary>
	[CustomPropertyDrawer(typeof(AnimatorStateNameAttribute))]
	public class AnimatorStateNamePropertyDrawer : PropertyDrawer
	{	
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var animatorStateNameAttribute = (AnimatorStateNameAttribute) attribute;
			SerializedProperty animatorProperty = EditorHelpers.FindPropertyRelative(property, 
												 animatorStateNameAttribute.animatorField);
			var animator = animatorProperty.objectReferenceValue as AnimatorController;
			if (animator == null)
			{
				base.OnGUI(position, property, label);
				return;
			}
			List<string> states = AnimatorStateCollector.CollectStatesNames(animator, 0, includeBlendTrees:false);
			string[] names = states.ToArray();

			string s = property.stringValue;
			int index = EditorGUI.Popup(position, property.displayName, GetIndex(s, names), names);
			property.stringValue = names[index];
		}

		private int GetIndex(string selected, IList<string> names)
		{
			int index = names.IndexOf(selected);
			return index < 0 ? 0 : index;
		}
	}
}
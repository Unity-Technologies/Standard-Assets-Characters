using System.Collections.Generic;
using Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace StandardAssets.Characters.Attributes.Editor
{
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
			for (int index = 0; index < names.Count; index++)
			{
				string stateName = names[index];
				if (stateName == selected)
				{
					return index;
				}
			}
			return 0;
		}
	}
}
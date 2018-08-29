using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomPropertyDrawer(typeof(IndentAttribute))]
	public class IndentPropertyDrawer : PropertyDrawer
	{
		private IndentAttribute IndentAttribute
		{
			get { return (IndentAttribute) attribute;  }
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

			EditorGUI.indentLevel += IndentAttribute.indentLevel;
			
			EditorGUI.PropertyField(position, property, label, true);
			
			EditorGUI.indentLevel -= IndentAttribute.indentLevel;
		}
	}
}


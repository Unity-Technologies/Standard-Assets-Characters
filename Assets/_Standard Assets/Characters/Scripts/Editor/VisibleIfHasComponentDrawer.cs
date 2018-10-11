using UnityEditor;
using UnityEngine;
using StandardAssets.Characters.Attributes;

namespace Editor
{
	/// <summary>
	/// Property Drawer for fields that make use of the <see cref="VisibleIfHasComponentAttribute"/>.
	/// </summary>
	[CustomPropertyDrawer(typeof(VisibleIfHasComponentAttribute))]
	public class VisibleIfHasComponentDrawer : PropertyDrawer
	{
		/// <summary>
		/// Gets the height of the property being drawn to the inspector to ensure correct formatting 
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetConditionalPropertyDrawerHeight
			(
				attribute as VisibleIfHasComponentAttribute,
				property
			);
		}

		/// <summary>
		/// Draws the relevant property using this attribute to the inspector when OnGui is triggered
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (position.height < 1.0f)
			{
				return;
			}
			EditorGUI.PropertyField(position, property, label, true);
		}
		
		/// <summary>
		/// Determine if the property should be shown and return the height if it's visible.
		/// Returns 0 if hidden.
		/// </summary>
		internal static float GetConditionalPropertyDrawerHeight(VisibleIfHasComponentAttribute attribute,
		                                                         SerializedProperty property)
		{
			if (attribute == null)
			{
				return 0.0f;
			}

			bool show = true;
			MonoBehaviour behaviour = property.serializedObject.targetObject as MonoBehaviour;
			if (behaviour != null)
			{
				show = behaviour.gameObject.GetComponent(attribute.componentType) != null;
			}
			
			return show ? EditorGUI.GetPropertyHeight(property) : 0.0f;
		}
	}
}
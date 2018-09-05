using StandardAssets.Characters.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// Property Drawer for fields that make use of the <see cref="VisibleIfAttribute"/>
	/// </summary>
	[CustomPropertyDrawer(typeof(VisibleIfAttribute))]
	public class VisibleIfPropertyDrawer : PropertyDrawer
	{
		/*
		 * (Codie) BugFix: All visibility and heights must be calculated in GetPropertyHeight, always called before OnGUI,
		 *     or it breaks layout rectangles. Previous logic set visibility in OnGUI,
		 *     causing GUILayout Rects to be one frame behind (GUI Overlapped draw).
		 */
		/// <summary>
		/// Gets the height of the property being drawn to the inspector to ensure correct formatting 
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetConditionalPropertyDrawerHeight
				(
					attribute as VisibleIfAttribute,
					property,
					label
				);
		}

		/// <summary>
		/// Draws the relevant property using this attribute to the inspector when OnGui is triggered
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (position.height < 1f)
			{
				return;
			}
			EditorGUI.PropertyField(position, property, label, true);
		}

		/// <summary>
		/// Static GetHeight implementation for compatibility with HelperBoxAttribute and possibly others.
		/// Returns 0 if hidden.
		/// </summary> 
		internal static float GetConditionalPropertyDrawerHeight(VisibleIfAttribute attribute,
																 SerializedProperty property, GUIContent label)
		{
			if (attribute == null)
			{
				return 0;
			}

			bool show = true;
			if (!string.IsNullOrEmpty(attribute.conditionField))
			{
				var conditionProperty = EditorHelpers.FindPropertyRelative(property, attribute.conditionField);
				if (conditionProperty != null)
				{
					bool isBoolMatch = conditionProperty.propertyType == SerializedPropertyType.Boolean &&
									   conditionProperty.boolValue;
					
					string compareStringValue = attribute.conditionElement == null
						? string.Empty
						: attribute.conditionElement.ToString().ToUpper();
					
					if (isBoolMatch && compareStringValue == "FALSE")
					{
						isBoolMatch = false;
					}
					string conditionPropertyStringValue = conditionProperty.AsStringValue().ToUpper();
					bool objectMatch = compareStringValue == conditionPropertyStringValue;

					if (!isBoolMatch && !objectMatch)
					{
						show = false;
					}
				}
			}

			return show ? EditorGUI.GetPropertyHeight(property) : 0;
		} 
	} 
}


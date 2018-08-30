using StandardAssets.Characters.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomPropertyDrawer(typeof(VisibleIfAttribute))]
	public class VisibleIfPropertyDrawer : PropertyDrawer
	{
		/*
		 * (Codie) BugFix: All visibility and heights must be calculated in GetPropertyHeight, always called before OnGUI,
		 *     or it breaks layout rectangles. Previous logic set visibility in OnGUI,
		 *     causing GUILayout Rects to be one frame behind (GUI Overlapped draw).
		 */
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetConditionalPropertyDrawerHeight
				(
					attribute as VisibleIfAttribute,
					property,
					label
				);
		}

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


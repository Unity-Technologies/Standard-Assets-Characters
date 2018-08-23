using System.Runtime.Remoting.Messaging;
using Attributes;
using UnityEditor;
using UnityEngine;
using System;
using PropertyAttribute = NUnit.Framework.PropertyAttribute;

namespace Editor
{

    [CustomPropertyDrawer(typeof(ConditionalIncludeAttribute))]
    public class ConditionalIncludePropertyDrawer : PropertyDrawer
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
                    this.attribute as ConditionalIncludeAttribute,
                    property,
                    label
                );
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (position.height > 0f)
            {
                EditorGUI.PropertyField(position, property, label, true);
            } 
        }


        /// <summary>
        /// Static GetHeight implementation for compatibility with HelperBoxAttribute and possibly others.
        /// Returns 0 if hidden.
        /// </summary> 
        internal static float GetConditionalPropertyDrawerHeight(ConditionalIncludeAttribute attribute, SerializedProperty property, GUIContent label)
        {
            if (attribute == null)
            {
                return 0;
            }
            bool show = true;
            if (!string.IsNullOrEmpty(attribute.conditionField))
            {
                var conditionProperty = FindPropertyRelative(property, attribute.conditionField);
                if (conditionProperty != null)
                {
                    bool isBoolMatch = conditionProperty.propertyType == SerializedPropertyType.Boolean && conditionProperty.boolValue;
                    string compareStringValue = attribute.conditionElement ==  null ? string.Empty  : attribute.conditionElement.ToString().ToUpper() ;
                    if (isBoolMatch && compareStringValue == "FALSE") isBoolMatch = false;

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

 
        
        private static SerializedProperty FindPropertyRelative(SerializedProperty property, string propertyName)
        {
            if (property.depth == 0) return property.serializedObject.FindProperty(propertyName);

            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            SerializedProperty parent = null;

		
            for (int i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                int index = -1;
                if (element.Contains("["))
                {
                    index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    element = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                }
			
                parent = i == 0 ? 
                    property.serializedObject.FindProperty(element) : 
                    parent.FindPropertyRelative(element);

                if (index >= 0) parent = parent.GetArrayElementAtIndex(index);
            }

            return parent.FindPropertyRelative(propertyName);
        }
        
    }

    public static class SerialisedPropertyExtensions
    {

        /// <summary>
        /// Helper to try find another attribute of a specified type.
        /// </summary> 
        public static T TryGetAttribute<T>(this PropertyDrawer drawer) where T : Attribute
        {
            object[] result = drawer.fieldInfo.GetCustomAttributes(typeof(T), true);
            if (result != null && result.Length > 0)
            {
                return result[0] as T;
            }
            return null;
        }
        
        
        public static string AsStringValue(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Character:
                case SerializedPropertyType.Integer:
                    if (property.type == "char") return Convert.ToChar(property.intValue).ToString();
                    return property.intValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue != null ? property.objectReferenceValue.ToString() : "null";
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Enum:
                    return property.enumNames[property.enumValueIndex];
                default:
                    return string.Empty;
            }
        }
    }
}

    

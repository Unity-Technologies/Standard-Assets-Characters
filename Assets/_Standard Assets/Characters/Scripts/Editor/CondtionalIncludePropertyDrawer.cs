using System.Runtime.Remoting.Messaging;
using Attributes;
using UnityEditor;
using UnityEngine;
using System;


namespace Editor
{

    [CustomPropertyDrawer(typeof(ConditionalIncludeAttribute))]
    public class ConditionalIncludePropertyDrawer : PropertyDrawer
    {
        private ConditionalIncludeAttribute includeAttribute
        {
            get{ return (ConditionalIncludeAttribute)attribute; } 
        }

        private bool toShow = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return toShow ? EditorGUI.GetPropertyHeight(property) : 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!string.IsNullOrEmpty(includeAttribute.conditionField))
            {
                var conditionProperty = FindPropertyRelative(property, includeAttribute.conditionField);
                if (conditionProperty != null)
                {
                    bool isBoolMatch = conditionProperty.propertyType == SerializedPropertyType.Boolean && conditionProperty.boolValue;
                    string compareStringValue = includeAttribute.conditionElement ==  null ? string.Empty  : includeAttribute.conditionElement.ToString().ToUpper() ;
                    if (isBoolMatch && compareStringValue == "FALSE") isBoolMatch = false;

                    string conditionPropertyStringValue = conditionProperty.AsStringValue().ToUpper();
                    bool objectMatch = compareStringValue == conditionPropertyStringValue;

                    if (!isBoolMatch && !objectMatch)
                    {
                        toShow = false;
                        return;
                    }
                }
            }

            toShow = true;
            EditorGUI.PropertyField(position, property, label, true);
        }

        private SerializedProperty FindPropertyRelative(SerializedProperty property, string propertyName)
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

    

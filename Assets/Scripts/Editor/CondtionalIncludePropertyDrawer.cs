using System.Runtime.Remoting.Messaging;
using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{

    [CustomPropertyDrawer(typeof(ConditionalIncludeAttribute))]
    public class ConditionalIncludePropertyDrawer : PropertyDrawer
    {
        private ConditionalIncludeAttribute includeAttribute
        {
            get{ return (ConditionalIncludeAttribute)attribute; } 
        }

        private bool showField;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            showField = false;
            if(includeAttribute.conditionElement != null)
            {
                var conditionIndex = property.serializedObject.FindProperty(includeAttribute.conditionField).enumValueIndex;
                showField = (int) includeAttribute.conditionElement == conditionIndex; 
            }
            else
            {
                showField = property.serializedObject.FindProperty(includeAttribute.conditionField).boolValue;
            }
            
            if (showField)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return showField ? EditorGUI.GetPropertyHeight(property) : 0;
        }
    }
}

    

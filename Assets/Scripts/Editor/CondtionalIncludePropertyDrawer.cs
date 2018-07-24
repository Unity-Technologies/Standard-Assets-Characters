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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var conditionIndex = property.serializedObject.FindProperty(includeAttribute.conditionField).enumValueIndex;

            if ((int) includeAttribute.enumElement == conditionIndex)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}

    

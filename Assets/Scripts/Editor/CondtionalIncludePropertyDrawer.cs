using Attributes;
using UnityEditor;
using UnityEngine;


namespace tfb.PropertyDrawers
{
    
#if UNITY_EDITOR
    
    public abstract class ToggleablePropertyDrawer : PropertyDrawer
    {
        protected bool GetToggleFromValueOrCondition(PropertyAttribute attribute, SerializedProperty property)
        {
            ConditionalIncludeAttribute toggleable = (ConditionalIncludeAttribute) attribute;

            bool? conditional = GetConditionalAttributeResult(toggleable, property);
            
            switch (conditional)
            {
                case true:
                    return true;

                case false:
                    return false;

                case null:
                default:
                    return toggleable.Toggle;
            }
        }

        private bool? GetConditionalAttributeResult(ConditionalIncludeAttribute toggleable, SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, toggleable.Condition);
            SerializedProperty sourceValue = property.serializedObject.FindProperty(conditionPath);

            if (sourceValue != null && sourceValue.propertyType == SerializedPropertyType.Boolean)
            {
                return sourceValue.boolValue;
            }

            return null;
        }
    }
    
    [CustomPropertyDrawer(typeof(ConditionalIncludeAttribute))]
    public class CondtionalIncludePropertyDrawer : ToggleablePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool toggle = GetToggleFromValueOrCondition(attribute, property);
    
            using (new EditorGUI.DisabledGroupScope(toggle))
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
#endif
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace Attributes
{
    public abstract class ConditionalIncludeAttribute : PropertyAttribute
    {
        public bool Toggle;
        public string Condition;

        public ConditionalIncludeAttribute()
        {
            Toggle = true;
        }

        public ConditionalIncludeAttribute(bool toggle)
        {
            Toggle = toggle;
        }

        public ConditionalIncludeAttribute(string condition)
        {
            Condition = condition;
        }
    }
    
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
}
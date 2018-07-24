using Attributes;
using UnityEditor;
using UnityEngine;


namespace tfb.PropertyDrawers
{
    
#if UNITY_EDITOR
    
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
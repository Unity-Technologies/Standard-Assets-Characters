using Attributes;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HelperBoxAttribute))]
    public class HelperBoxPropertyDrawer : PropertyDrawer
    {
        private float height = 0;
        private float textHeight = 0;
        
        private HelperBoxAttribute helpAttribute
        {
            get { return (HelperBoxAttribute) attribute; }
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            height = base.GetPropertyHeight(prop, label);
            var content = new GUIContent(helpAttribute.text);
            var style = GUI.skin.GetStyle("helpbox");
            textHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - 100);

            return height + textHeight;
        }


        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, prop);
	        
            var helpPos = position;
            helpPos.height = textHeight;

            EditorGUI.HelpBox(helpPos, helpAttribute.text, (MessageType)helpAttribute.type);
            position.height = height;

            position.y += helpPos.height;
                EditorGUI.PropertyField(position, prop, label);

            EditorGUI.EndProperty();
        }
    }
#endif
}
using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HelperBoxAttribute))]
    public class HelperBoxPropertyDrawer : PropertyDrawer
    {
        float height = 0;
        int paddingHeight = 8;
        int marginHeight = 2;
        float additionalHeight = 0;

        HelperBoxAttribute helpAttribute
        {
            get { return (HelperBoxAttribute) attribute; }
        }

        MultilineAttribute multilineAttribute
        {
            get
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(MultilineAttribute), true);
                return attributes != null && attributes.Length > 0 ? (MultilineAttribute) attributes[0] : null;
            }
        }


        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            height = base.GetPropertyHeight(prop, label);

            float minHeight = paddingHeight * 5;

            var content = new GUIContent(helpAttribute.text);
            var style = GUI.skin.GetStyle("helpbox");

            var newHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth);

            height += marginHeight * 2;

            if (multilineAttribute != null && prop.propertyType == SerializedPropertyType.String)
            {
                additionalHeight = 48f;
            }

            return height > minHeight ? height + height + additionalHeight : minHeight + height + additionalHeight;
        }


        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            var multiline = multilineAttribute;

            EditorGUI.BeginProperty(position, label, prop);

            var helpPos = position;
            helpPos.height -= height + marginHeight;


            if (multiline != null)
            {
                helpPos.height -= additionalHeight;
            }

            EditorGUI.HelpBox(helpPos, helpAttribute.text, helpAttribute.type);

            position.y += helpPos.height + marginHeight;
            position.height = height;


            if (multiline != null)
            {
                if (prop.propertyType == SerializedPropertyType.String)
                {
                    var style = GUI.skin.label;
                    var size = style.CalcHeight(label, EditorGUIUtility.currentViewWidth);

                    EditorGUI.LabelField(position, label);

                    position.y += size;
                    position.height += additionalHeight - size;

                    prop.stringValue = EditorGUI.TextArea(position, prop.stringValue);
                }
                else
                {
                    EditorGUI.PropertyField(position, prop, label);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, prop, label);
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
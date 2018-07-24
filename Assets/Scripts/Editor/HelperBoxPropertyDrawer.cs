using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(HelperBoxAttribute))]
    public class HelperBoxPropertyDrawer : DecoratorDrawer
    {
        HelperBoxAttribute helperBoxAttribute { get { return ((HelperBoxAttribute)attribute); } }
        public override void OnGUI(Rect _position)
        {
           
            
            if(helperBoxAttribute.text == "")
            {
                _position.height = 1;
                _position.y += 5;
                GUI.Box(_position, "");
            } else
            {
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(helperBoxAttribute.text));
                float separatorWidth = (_position.width - textSize.x) / 15f;
                _position.y += 5;
                
                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(helperBoxAttribute.text, GUILayout.MaxWidth(50f));
            }
        }
        
        public override float GetHeight()
        {
            return 41.0f;
        }
    }
}
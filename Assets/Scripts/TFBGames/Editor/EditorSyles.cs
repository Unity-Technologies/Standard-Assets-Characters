using UnityEngine;
using UnityEditor;

namespace TFBGames.Editor
{
    /// <summary>
    /// Editor Styles
    /// </summary>
    static class TBFEditorStyles
    {
        static bool isInitialized;

        static GUIStyle
            deleteArrayItemButtonStyle,
            panelHeader,
            panelContent,
            panelItem;





        /// <summary>
        /// A small button for deleting array elements. Intended for horizontal layout without expanded widths.
        /// </summary> 
        public static GUIStyle DeletArrayItemButton
        {
            get
            {
                return PassCheckInitialized(deleteArrayItemButtonStyle);
            }
        }

        /// <summary>
        /// Array Item styles for "RL" prefixed editor styles.
        /// </summary>
        public static GUIStyle PanelArrayItem
        {
            get 
            {
                return PassCheckInitialized(panelItem);
            }
        }




        static GUIStyle PassCheckInitialized(GUIStyle style)
        {
            if(!isInitialized)
            {
                isInitialized = true;

                // a new copy is needed if the style is modified, otherwise you can break unity's internal styles.
                deleteArrayItemButtonStyle = new GUIStyle("button");
                deleteArrayItemButtonStyle.fixedWidth = 24;
                deleteArrayItemButtonStyle.alignment = TextAnchor.MiddleCenter;
                deleteArrayItemButtonStyle.stretchWidth = false;

                // RL prefixed style elements are the kind used by UnityEvent inspectors and some list inspectors.
                // we are using it to construct panels in the same style.
                panelHeader     = "RL Header";
                panelContent    = new GUIStyle( "RL Background");
                panelContent.fixedHeight = 0;
                panelContent.stretchHeight = false;
                panelItem       = "RL Element"; 
            }
            return style;
        }


        public static void DrawPanel(GUIContent title, System.Action content)
        {
            if (!isInitialized)
                PassCheckInitialized(null);
            
            // container for entire panel
            EditorGUILayout.BeginVertical();
            { // braces for readability

                // Header:
                EditorGUILayout.BeginHorizontal(panelHeader);
                EditorGUILayout.LabelField(title);
                // we can add things to header here if needed.
                EditorGUILayout.EndHorizontal();
                // Content:
                EditorGUILayout.BeginVertical(panelContent);
                EditorGUILayout.BeginVertical();
                content();
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

    }

}
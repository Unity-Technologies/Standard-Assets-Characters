using System.Collections.Generic;
using StandardAssets.Characters.Editor;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours.Editor
{
    /// <summary>
    /// Custom Editor for <see cref="LocomotionAnimatorState"/>
    /// </summary>
    [CustomEditor(typeof(LocomotionAnimatorState))]
    public class LocomotionStateEditor : UnityEditor.Editor
    {
        const string k_MovementConfig = "m_MovementConfig";
        const string k_Script = "m_Script";
        
        /// <summary>
        /// Draws the <see cref="LocomotionAnimatorState.m_MovementConfig"/> ScriptableObject so it can be edited here./>
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, k_MovementConfig);
            GUILayout.Space(10);
			
            serializedObject.DrawExtendedScriptableObject(k_MovementConfig, "Movement Config");
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the <see cref="LocomotionAnimatorState.m_MovementConfig"/> ScriptableObject so it can be edited
        /// but hides the script field./>
        /// </summary>
        public void DrawScriptableObject()
        {
            DrawPropertiesExcluding(serializedObject, k_MovementConfig, k_Script);
			
            serializedObject.DrawExtendedScriptableObject(k_MovementConfig, 
                "Movement Config", !Application.isPlaying);
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

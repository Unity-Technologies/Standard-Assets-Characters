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
    }
}

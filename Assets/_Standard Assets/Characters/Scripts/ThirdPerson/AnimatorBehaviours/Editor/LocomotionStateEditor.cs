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
        
        public void OnEnable()
        {
            var locomotionAnimator = (LocomotionAnimatorState)target;
            var context = UnityEditor.Animations.AnimatorController.FindStateMachineBehaviourContext(locomotionAnimator);
            if (context != null && context.Length > 0)
            {
                var state = context[0].animatorObject as UnityEditor.Animations.AnimatorState;
                if (state != null)
                {
                    locomotionAnimator.stateName = state.name;
                }
            };
        }
        
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

        public void DrawScriptableObject()
        {
            DrawPropertiesExcluding(serializedObject, k_MovementConfig, "m_Script");
			
            serializedObject.DrawExtendedScriptableObject(k_MovementConfig, "Movement Config");
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

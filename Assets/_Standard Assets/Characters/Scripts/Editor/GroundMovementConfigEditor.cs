using System.Collections.Generic;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.Editor
{
    /// <summary>
    /// Custom Editor for <see cref="MotorConfig"/>
    /// </summary>
    [CustomEditor(typeof(GroundMovementConfig))]
    public class GroundMovementConfigEditor : UnityEditor.Editor
    {
        //Property names
        const string k_RootMotionScale = "m_RootMotionScale";
        const string k_MaxSpeed = "m_MaxSpeed";
        const string k_MovementSpeedDelta = "m_MovementSpeedDelta";
		
        /// <summary>
        /// Draws the inspector GUI using exclusions
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUI.indentLevel++;
            DrawPropertiesExcluding(serializedObject, GetExclusions());
            EditorGUI.indentLevel--;
			
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
		
		
        /// <summary>
        /// Gets the properties that should not be rendered
        /// </summary>
        string[] GetExclusions()
        {
            var config =  (GroundMovementConfig)target;
            var exclusions = config.useRootMotion ? 
                new List<string> { k_MovementSpeedDelta, k_MaxSpeed } : 
                new List<string> { k_RootMotionScale };
            return exclusions.ToArray();
        }
    }
}
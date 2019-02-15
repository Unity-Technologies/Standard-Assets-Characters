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
        const string k_MaxSpeedCurve = "m_MaxSpeedCurve";
        const string k_SpeedDelta = "m_SpeedDelta";
        const string k_MaxSpeed = "m_MaxSpeed";
        const string k_MaxSpeedType = "m_MaxSpeedType";
		
        /// <summary>
        /// Draws the inspector GUI using exclusions
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, GetExclusions());
			
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
            var exclusions = new List<string>();
            if (config.useRootMotion)
            {
                exclusions = new List<string> { k_SpeedDelta, k_MaxSpeed, k_MaxSpeedType, k_MaxSpeedCurve };
            }
            else
            {
                exclusions.Add(k_RootMotionScale);
                exclusions.Add(config.isMaxSpeedValue ? k_MaxSpeedCurve : k_MaxSpeed);
            }
            return exclusions.ToArray();
        }
        
        
    }
}
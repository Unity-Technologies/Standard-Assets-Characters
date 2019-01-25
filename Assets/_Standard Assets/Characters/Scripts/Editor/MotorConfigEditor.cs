using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.Editor
{
	/// <summary>
	/// Custom Editor for <see cref="MotorConfigEditor"/>
	/// </summary>
	[CustomEditor(typeof(MotorConfig))]
	public class MotorConfigEditor : UnityEditor.Editor
	{
		//Property names
		const string k_DefaultGroundMovementConfig = "m_DefaultGroundMovementConfig";
		const string k_Script = "m_Script";
		const string k_Help = "Setup movement configs per locomotion state on the animator otherwise this default will be used.";
		
		/// <summary>
		/// Draws the inspector GUI using exclusions
		/// </summary>
		public override void OnInspectorGUI()
		{
			EditorGUILayout.ObjectField(serializedObject.FindProperty(k_Script));
			EditorGUILayout.HelpBox(k_Help, MessageType.Info);
			serializedObject.DrawExtendedScriptableObject(k_DefaultGroundMovementConfig);
			EditorGUILayout.Space();
			DrawPropertiesExcluding(serializedObject, k_DefaultGroundMovementConfig, k_Script);
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
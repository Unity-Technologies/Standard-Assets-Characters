using StandardAssets.Characters.ThirdPerson.AnimatorBehaviours;
using StandardAssets.Characters.ThirdPerson.AnimatorBehaviours.Editor;
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

		MotorConfig m_MotorConfig;

		void OnEnable()
		{
			m_MotorConfig = (MotorConfig)target;
		}

		/// <summary>
		/// Draws the inspector GUI using exclusions
		/// </summary>
		public override void OnInspectorGUI()
		{
			var script = serializedObject.FindProperty(k_Script);
			EditorGUILayout.ObjectField(script);
			EditorGUILayout.LabelField("Ground Motion", new GUIStyle { fontStyle = FontStyle.Bold });
			serializedObject.DrawExtendedScriptableObject(k_DefaultGroundMovementConfig, "Default Config");

			script.isExpanded = EditorGUILayout.Foldout(script.isExpanded,
				"Animation State Configs");
			
			if (script.isExpanded && m_MotorConfig.animator != null)
			{
				EditorGUI.indentLevel++;
				foreach (var locomotionState in m_MotorConfig.animator.GetBehaviours<LocomotionAnimatorState>())
				{
					EditorGUILayout.LabelField(locomotionState.stateName, new GUIStyle{fontStyle = FontStyle.Bold});
					if (locomotionState.movementConfig == null)
					{
						EditorGUILayout.LabelField("No GroundMovementConfig assigned. You can assign one on the Locomotion Animator State behaviour.");
					}
					else
					{
						var editor = (LocomotionStateEditor)CreateEditor(locomotionState);
						editor.DrawScriptableObject();
						editor.serializedObject.ApplyModifiedProperties();
					}
					
					EditorGUILayout.Space();
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Space();
			DrawPropertiesExcluding(serializedObject, k_DefaultGroundMovementConfig, k_Script);
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
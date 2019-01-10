using System.Linq;
using StandardAssets.Characters.ThirdPerson;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.Editor
{
	/// <summary>
	/// Custom editor for <see cref="ThirdPersonBrain"/>
	/// </summary>
	[CustomEditor(typeof(ThirdPersonBrain))]
	public class ThirdPersonBrainEditor : UnityEditor.Editor
	{
		// Field names/paths
		const string k_AnimationTurnaroundName 		= "m_AnimationTurnAround";
		const string k_BlendspaceTurnaroundName 	= "m_BlendspaceTurnAround";
		const string k_AnimationConfigName 			= "m_Configuration";
		const string k_MotorName 					= "m_Motor";
		const string k_GizmoSettings 				= "m_GizmoSettings";
		const string k_MovementEvent 				= "m_MovementEffects";
		const string k_AdapterName 					= "m_OCCSettings";
		const string k_MotorConfigPath 				= "m_Motor.m_Configuration";

		// Fields that make up the advance section
		readonly string[] m_AdvancedFields = {k_AdapterName, k_MovementEvent, k_GizmoSettings};

		// Help box text
		const string k_Help =
			"Configurations are separate assets (ScriptableObjects). Click on the associated configuration to locate it in the Project View. Values can be edited here during runtime and not be lost. It also allows one to create different settings and swap between them. To create a new setting Right click -> Create -> Standard Assets -> Characters -> ...";

		// Tracks the whether the advanced foldout is open/collapse
		bool m_AdvancedFoldOut;


		/// <summary>
		/// Renders the custom editor 
		/// </summary>
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox(k_Help, MessageType.Info);
			DrawPropertiesExcluding(serializedObject, GetExclusions());
			serializedObject.DrawFieldsUnderFoldout("Advanced", m_AdvancedFields, ref m_AdvancedFoldOut, 
			                                        DrawTurnaround);
			
			serializedObject.DrawExtendedScriptableObject(k_MotorConfigPath, "Motor Settings");
			serializedObject.DrawExtendedScriptableObject(k_AnimationConfigName, "Animation Settings");
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		/// <summary>
		/// Returns the exclusion list
		/// </summary>
		string[] GetExclusions()
		{
			string[] exclusionList = 
			{
				k_AnimationTurnaroundName,
				k_BlendspaceTurnaroundName,
				k_AnimationConfigName,
				k_MotorName,
			};
			return exclusionList.Concat(m_AdvancedFields).ToArray();
		}

		// Renders the TurnAround properties based on the type
		void DrawTurnaround()
		{
			ThirdPersonBrain brain = (ThirdPersonBrain)target;

			switch (brain.typeOfTurnAround)
			{
				case TurnAroundType.Animation:
					EditorGUILayout.PropertyField(serializedObject.FindProperty(k_AnimationTurnaroundName), true);
					break;
				case TurnAroundType.Blendspace:
					EditorGUILayout.PropertyField(serializedObject.FindProperty(k_BlendspaceTurnaroundName), true);
					break;
			}
		}
	}
}
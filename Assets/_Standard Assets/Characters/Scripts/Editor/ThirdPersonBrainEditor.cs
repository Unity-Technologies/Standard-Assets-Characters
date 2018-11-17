using System.Linq;
using StandardAssets.Characters.ThirdPerson;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// COMMENT TODO
	/// </summary>
	[CustomEditor(typeof(ThirdPersonBrain))]
	public class ThirdPersonBrainEditor : CharacterBrainEditor
	{
		// COMMENT TODO
		const string k_AnimationTurnaroundName 		= "m_AnimationTurnAroundBehaviour";
		const string k_BlendspaceTurnaroundName 	= "m_BlendspaceTurnAroundBehaviour";
		const string k_AnimationConfigName 			= "m_Configuration";
		const string k_MotorName 					= "m_Motor";
		const string k_GizmoSettings 				= "m_GizmoSettings";
		const string k_MovementEvent 				= "m_ThirdPersonMovementEventHandler";
		const string k_AdapterName 					= "m_CharacterControllerAdapter";
		const string k_MotorConfigPath 				= "m_Motor.m_Configuration";

		// COMMENT TODO
		readonly string[] m_AdvancedFields = {k_AdapterName, k_MovementEvent, k_GizmoSettings};

		// COMMENT TODO
		const string k_Help =
			"Configurations are separate assets (ScriptableObjects). Click on the associated configuration to locate it in the Project View. Values can be edited here during runtime and not be lost. It also allows one to create different settings and swap between them. To create a new setting Right click -> Create -> Standard Assets -> Characters -> ...";

		// COMMENT TODO
		bool m_AdvancedFoldOut;


		/// <summary>
		/// COMMENT TODO
		/// </summary>
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox(k_Help, MessageType.Info);
			base.OnInspectorGUI();
			serializedObject.DrawFieldsUnderFoldout("Advanced", m_AdvancedFields, ref m_AdvancedFoldOut, 
			                                        DrawTurnaround);
			
			serializedObject.DrawExtendedScriptableObject(k_MotorConfigPath, "Motor Settings");
			serializedObject.DrawExtendedScriptableObject(k_AnimationConfigName, "Animation Settings");
		}

		/// <summary>
		/// COMMENT TODO
		/// </summary>
		protected override string[] GetExclusions()
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

		// COMMENT TODO
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
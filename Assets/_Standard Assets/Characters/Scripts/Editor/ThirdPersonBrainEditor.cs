using System.Linq;
using StandardAssets.Characters.ThirdPerson;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(ThirdPersonBrain))]
	public class ThirdPersonBrainEditor : CharacterBrainEditor
	{
		const string k_AnimationTurnaroundName = "m_AnimationTurnAroundBehaviour",
		                       k_BlendspaceTurnaroundName = "m_BlendspaceTurnAroundBehaviour",
		                       k_AnimationConfigName = "m_Configuration",
		                       k_MotorName = "m_Motor",
		                       k_GizmoSettings = "m_GizmoSettings",
		                       k_MovementEvent = "m_ThirdPersonMovementEventHandler",
		                       k_AdapterName = "m_CharacterControllerAdapter";
		string m_MotorConfigPath = string.Format("{0}.{1}", k_MotorName, k_AnimationConfigName);

		readonly string[] m_AdvancedFields = {k_AdapterName, k_MovementEvent, k_GizmoSettings};

		const string k_Help =
			"Configurations are separate assets (ScriptableObjects). Click on the associated configuration to locate it in the Project View. Values can be edited here during runtime and not be lost. It also allows one to create different settings and swap between them. To create a new setting Right click -> Create -> Standard Assets -> Characters -> ...";

		bool m_AdvancedFoldOut;

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox(k_Help, MessageType.Info);
			base.OnInspectorGUI();
			serializedObject.DrawFieldsUnderFoldout("Advanced", m_AdvancedFields, ref m_AdvancedFoldOut, 
			                                        DrawTurnaround);
			
			serializedObject.DrawExtendedScriptableObject(m_MotorConfigPath, "Motor Settings");
			serializedObject.DrawExtendedScriptableObject(k_AnimationConfigName, "Animation Settings");
		}

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
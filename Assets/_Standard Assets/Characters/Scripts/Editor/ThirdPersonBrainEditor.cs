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
		string k_MotorConfigPath = string.Format("{0}.{1}", k_MotorName, k_AnimationConfigName);

		readonly string[] advancedFields = new[] {k_AdapterName, k_MovementEvent, k_GizmoSettings};

		protected const string k_Help =
			"Configurations are separate assets (ScriptableObjects). Click on the associated configuration to locate it in the Project View. Values can be edited here during runtime and not be lost. It also allows one to create different settings and swap between them. To create a new setting Right click -> Create -> Standard Assets -> Characters -> ...";

		bool advancedFoldOut;

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox(k_Help, MessageType.Info);
			base.OnInspectorGUI();
			
			serializedObject.DrawExtendedScriptableObject(k_MotorConfigPath, "Motor Settings");
			serializedObject.DrawExtendedScriptableObject(k_AnimationConfigName, "Animation Settings");

			GUI.Box(EditorGUILayout.BeginVertical(), GUIContent.none);
			advancedFoldOut = EditorGUILayout.Foldout(advancedFoldOut, "Advanced Settings");
			if (advancedFoldOut)
			{
				EditorGUI.indentLevel++;
				DrawTurnaround();
				foreach (var propertyPath in advancedFields)
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyPath), true);
				}
				EditorGUI.indentLevel--;
			}
			GUILayout.EndHorizontal();
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
			return exclusionList.Concat(advancedFields).ToArray();
		}

		void DrawTurnaround()
		{
			ThirdPersonBrain brain = (ThirdPersonBrain)target;

			switch (brain.typeOfTurnaround)
			{
				case TurnaroundType.Animation:
					EditorGUILayout.PropertyField(serializedObject.FindProperty(k_AnimationTurnaroundName), true);
					break;
				case TurnaroundType.Blendspace:
					EditorGUILayout.PropertyField(serializedObject.FindProperty(k_BlendspaceTurnaroundName), true);;
					break;
			}
		}
	}
}
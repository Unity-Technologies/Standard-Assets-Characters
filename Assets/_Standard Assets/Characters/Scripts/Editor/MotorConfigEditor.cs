using System;
using System.Collections.Generic;
using StandardAssets.Characters.ThirdPerson;
using StandardAssets.Characters.ThirdPerson.AnimatorBehaviours;
using StandardAssets.Characters.ThirdPerson.AnimatorBehaviours.Editor;
using StandardAssets.Characters.ThirdPerson.Configs;
using UnityEditor;
using UnityEditor.Animations;
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
		const string k_AlwaysUseDefaultConfig = "m_AlwaysUseDefaultConfig";

		GUIStyle m_BoldLabelStyle;
		MotorConfig m_MotorConfig;
		
		/// <summary>
		/// Draws the inspector GUI using exclusions
		/// </summary>
		public override void OnInspectorGUI()
		{
			m_BoldLabelStyle = new GUIStyle
			{
				fontStyle = FontStyle.Bold,
				normal = { textColor = GUI.skin.label.normal.textColor },
			};
			
			var script = serializedObject.FindProperty(k_Script);
			EditorGUILayout.ObjectField(script);
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Ground Motion", m_BoldLabelStyle);

			EditorGUILayout.PropertyField(serializedObject.FindProperty(k_AlwaysUseDefaultConfig));
			serializedObject.DrawExtendedScriptableObject(k_DefaultGroundMovementConfig, "Default Config");

			if (m_MotorConfig.alwaysUseDefaultConfig)
			{
				EditorGUI.BeginDisabledGroup(true);
			}
			
			var selected = Selection.activeGameObject;
			if (selected != null)
			{
				var current = selected.GetComponent<ThirdPersonBrain>();
				if (current != null)
				{
					var foldout = script.isExpanded;
					DrawAnimatorStateConfigs(current.GetComponent<Animator>(), ref foldout);
					script.isExpanded = foldout;
				}
			}
			
			if (m_MotorConfig.alwaysUseDefaultConfig)
			{
				EditorGUI.EndDisabledGroup();
			}

			EditorGUILayout.Space();
			DrawPropertiesExcluding(serializedObject, k_DefaultGroundMovementConfig, k_Script,
				k_AlwaysUseDefaultConfig);

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		void DrawAnimatorStateConfigs(Animator animator, ref bool foldout)
		{
			if (animator != null)
			{
				foldout = EditorGUILayout.Foldout(foldout,	"Animation State Configs");
				if (foldout)
				{
					var dict = new Dictionary<AnimatorState, LocomotionAnimatorState>();
					var animatorController = (AnimatorController)animator.runtimeAnimatorController;
					foreach (var layer in animatorController.layers)
					{
						TraverseStatemachineToFindBehaviour(layer.stateMachine, ref dict);
					}
					if (dict.Count == 0)
					{
						EditorGUILayout.LabelField("No LocomotionAnimatorStates could be found. You can add these " +
							"to the locomotion animator states in the Animator.", m_BoldLabelStyle);
					}
					else
					{
						EditorGUI.indentLevel++;
						foreach (var keyValuePair in dict)
						{
							var locomotionState = keyValuePair.Value;
							EditorGUILayout.LabelField(keyValuePair.Key.name, m_BoldLabelStyle);
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
				}
			}
		}

		void OnEnable()
		{
			m_MotorConfig = (MotorConfig)target;
		}

		/// <summary>
		/// Traverses the given state machine to find all the StateMachineBehaviours of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="stateMachine">StateMachine to traverse.</param>
		/// <param name="dict">Dictionary to populate with the results.</param>
		/// <typeparam name="T">StateMachineBehaviour type to find.</typeparam>
		static void TraverseStatemachineToFindBehaviour<T>(AnimatorStateMachine stateMachine, 
			ref Dictionary<AnimatorState, T> dict) where T : StateMachineBehaviour
		{
			foreach (var state in stateMachine.states)
			{
				foreach (var behaviour in state.state.behaviours)
				{
					var value = behaviour as T;
					if (value != null)
					{
						dict.Add(state.state, value);
						break;
					}
				}
			}
			
			foreach (var childStateMachine in stateMachine.stateMachines)
			{
				TraverseStatemachineToFindBehaviour<T>(childStateMachine.stateMachine, ref dict);
			}
		}

	}
}

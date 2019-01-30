using System;
using System.Collections.Generic;
using System.Linq;
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

		/// <summary>
		/// Draws the inspector GUI using exclusions
		/// </summary>
		public override void OnInspectorGUI()
		{
			var boldLabelStyle = new GUIStyle
			{
				fontStyle = FontStyle.Bold,
				normal = { textColor = GUI.skin.label.normal.textColor },
			};

			var script = serializedObject.FindProperty(k_Script);
			EditorGUILayout.ObjectField(script);
			EditorGUILayout.LabelField("Ground Motion", boldLabelStyle);
			serializedObject.DrawExtendedScriptableObject(k_DefaultGroundMovementConfig, "Default Config");


			var current = Selection.activeGameObject.GetComponent<ThirdPersonBrain>();
			if (current != null)
			{
				var animator = current.GetComponent<Animator>();
				if (animator != null)
				{
					script.isExpanded = EditorGUILayout.Foldout(script.isExpanded,
						"Animation State Configs");
					if (script.isExpanded)
					{
						var dict = new Dictionary<AnimatorState, LocomotionAnimatorState>();
						var animatorController = (AnimatorController)animator.runtimeAnimatorController;
						foreach (var layer in animatorController.layers)
						{
							TraverseStatemachineToFindBehaviour(layer.stateMachine, ref dict);
						}
						if (dict.Count == 0)
						{
							EditorGUILayout.LabelField("No LocomotionAnimatorStates could be found. Set them up " +
								"in the Animator.", boldLabelStyle);
						}
						else
						{
							EditorGUI.indentLevel++;
							foreach (var keyValuePair in dict)
							{
								var locomotionState = keyValuePair.Value;
								EditorGUILayout.LabelField(keyValuePair.Key.name, boldLabelStyle);
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

			EditorGUILayout.Space();
			DrawPropertiesExcluding(serializedObject, k_DefaultGroundMovementConfig, k_Script);

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

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

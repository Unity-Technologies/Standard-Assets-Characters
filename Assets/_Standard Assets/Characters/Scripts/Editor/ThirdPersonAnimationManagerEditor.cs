using System.Collections.Generic;
using StandardAssets.Characters.ThirdPerson;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(ThirdPersonCameraAnimationManager))]
	public class ThirdPersonAnimationManagerEditor : UnityEditor.Editor
	{
		private static readonly string[] s_Exclude =
		{
			"explorationCameraStates",
			"strafeCameraStates"
		};

		private readonly List<string> controllerStatesNames = new List<string>();

		private ThirdPersonCameraAnimationManager manager;
		private Animator animator;
		private AnimatorController controller;

		public override void OnInspectorGUI()
		{
			RefreshTarget();

			serializedObject.UpdateIfRequiredOrScript();

			EditorGUI.BeginChangeCheck();
			DrawPropertiesExcluding(serializedObject, s_Exclude);

			if (controller == null)
			{
				EditorGUILayout.HelpBox("There is no Animator Controller assigned", MessageType.Error);
			}
			else if (controller.layers.Length < 1)
			{
				EditorGUILayout.HelpBox("Animator Controller is empty", MessageType.Error);
			}

			EditorGUILayout.Space();

			// the two properties we are drawing manually:
			//DrawStateNamesProperty(serializedObject.FindProperty(s_Exclude[0]));
			//DrawStateNamesProperty(serializedObject.FindProperty(s_Exclude[1]));

			DrawStateProperty(serializedObject.FindProperty(s_Exclude[0]));
			DrawStateProperty(serializedObject.FindProperty(s_Exclude[1]));

			bool b = EditorGUI.EndChangeCheck();
			if (b)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		private void RefreshTarget()
		{
			manager = target as ThirdPersonCameraAnimationManager;
			if (manager != null)
			{
				animator = manager.GetComponent<Animator>();
			}
			else
			{
				animator = null;
			}

			controller = animator == null ? null : animator.runtimeAnimatorController as AnimatorController;
		}

		private void DrawStateProperty(SerializedProperty property)
		{
			FindStateNames(controllerStatesNames, controller); 
			EditorHelpers.DrawArrayPropertyPanel(
				property, item => { item.stringValue = DrawNextStatePopup(item.stringValue); });
		}
	
		private string DrawNextStatePopup(string selected)
		{
			// manual label here because we do not want it to expand
			//EditorGUILayout.LabelField(displayName, GUILayout.ExpandWidth(false)/*, GUILayout.MaxWidth(24)*/);
			if (controllerStatesNames.Count < 1)
			{
				// We have no popup values, so we will just show a delayed text field instead:
				selected = EditorGUILayout.DelayedTextField(selected);
				return selected;
			}

			// If for some reason te state doesn't exist, allow the value to remain until user changes it.
			int sel = FindIndex(selected);
			if (sel < 0)
			{
				controllerStatesNames.Insert(0, selected.Trim());
				sel = 0;
			}

			sel = EditorGUILayout.Popup(sel, controllerStatesNames.ToArray());
			selected = controllerStatesNames[sel];

			// Remove selected string so it cannot be selected twice in followup popups.
			//controllerStatesNames.Remove(selected);

			return selected;
		}

		private int FindIndex(string p)
		{
			for (int i = 0; i < controllerStatesNames.Count; i++)
			{
				if (string.Equals(p, controllerStatesNames[i]))
				{
					return i;
				}
			}

			return -1;
		}

		private void FindStateNames(List<string> result, AnimatorController c)
		{
			result.Clear();
			if (c == null)
			{
				return;
			}

			AnimatorControllerLayer[] layers = c.layers;
			foreach (AnimatorControllerLayer layer in layers)
			{
				FindStateNames(result, layer.stateMachine);
			}
		}

		private void FindStateNames(List<string> result, AnimatorStateMachine machine)
		{
			ChildAnimatorState[] states = machine.states;
			foreach (ChildAnimatorState childState in states)
			{
				AnimatorState state = childState.state;
				if (!result.Contains(state.name))
				{
					result.Add(state.name);
				}
			}
		}
	}
}
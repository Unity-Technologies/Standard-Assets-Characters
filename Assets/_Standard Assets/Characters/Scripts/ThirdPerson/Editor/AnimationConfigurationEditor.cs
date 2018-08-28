using System.Collections.Generic;
using Editor;
using UnityEditor;
using UnityEditor.Animations;

namespace StandardAssets.Characters.ThirdPerson.Editor
{
	[CustomEditor(typeof(ThirdPersonAnimationConfiguration))]
	public class AnimationConfigurationEditor : UnityEditor.Editor
	{
		private static readonly string[] s_Exclude =
		{
			"locomotion",
			"rightFootRootMotionJump",
			"leftFootRootMotionJump",
			"rightFootJump",
			"leftFootJump",
			"rollLand",
			"land"
		};
		
		private ThirdPersonAnimationConfiguration config;
		private string[] names;
		
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.LabelField("Animation States:");
			if (config.animator == null)
			{
				EditorGUILayout.LabelField("An animator is required to edit states!");
			}
			else
			{
				List<string> states = AnimatorStateCollector.CollectStatesNames(config.animator as AnimatorController,
																				0, includeBlendTrees: false);
				names = states.ToArray();
				
				int locomotion = EditorGUILayout.Popup("Locomotion", GetIndex(config.locomotionStateName), names);
				SetStringValue("locomotion", names[locomotion]);
				
				int rightFootRootMotionJump = EditorGUILayout.Popup("Right Foot Root Motion Jump", GetIndex(config.rightFootRootMotionJumpStateName), names);
				SetStringValue("rightFootRootMotionJump", names[rightFootRootMotionJump]);
				
				int leftFootRootMotionJump = EditorGUILayout.Popup("Left Foot Root Motion Jump", GetIndex(config.leftFootRootMotionJumpStateName), names);
				SetStringValue("leftFootRootMotionJump", names[leftFootRootMotionJump]);
				
				int rightFootJump = EditorGUILayout.Popup("Right Foot Jump", GetIndex(config.rightFootJumpStateName), names);
				SetStringValue("rightFootJump", names[rightFootJump]);
				
				int leftFootJump = EditorGUILayout.Popup("Left Foot Jump", GetIndex(config.leftFootJumpStateName), names);
				SetStringValue("leftFootJump", names[leftFootJump]);
				
				int rollLand = EditorGUILayout.Popup("Roll Land", GetIndex(config.rollLandStateName), names);
				SetStringValue("rollLand", names[rollLand]);
				
				int land = EditorGUILayout.Popup("Land", GetIndex(config.landStateName), names);
				SetStringValue("land", names[land]);
				
				EditorGUILayout.Space();
			}
			
			DrawPropertiesExcluding(serializedObject, s_Exclude);
			
			bool changed = EditorGUI.EndChangeCheck();
			if (changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		private void SetStringValue(string propertyName, string value)
		{
			SerializedProperty property = serializedObject.FindProperty(propertyName);
			property.stringValue = value;
		}
		
		private int GetIndex(string selected)
		{
			for (int index = 0; index < names.Length; index++)
			{
				string stateName = names[index];
				if (stateName == selected)
				{
					return index;
				}
			}
			return 0;
		}

		private void OnEnable()
		{
			config = (ThirdPersonAnimationConfiguration) target;
		}
	}
}

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TFBGames.Editor
{
	public class AnimationControllerEditor : EditorWindow
	{
		[Serializable]
		public struct ReplacementRule
		{
			public string oldValue,
			              newValue;
		}
	
		private AnimatorController baseController;
		private bool useBaseClipsIfNotFound;
		private string newControllerName = "NewAnimationController";
		public ReplacementRule[] clipNameReplacementRules;

		// Add menu item named "My Window" to the Window menu
		[MenuItem("24 Bit Games/Animation Controller Clip Replacer")]
		public static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			GetWindow(typeof(AnimationControllerEditor));
		}

		private void ReplaceAnimationClips(BlendTree blendtree)
		{
			if (blendtree == null)
			{
				return;
			}

			if (blendtree.children[0].motion is AnimationClip)
			{
				int length = blendtree.children.Length;
				var newChildren = new ChildMotion[length];
				for (int i = 0; i < length; i++)
				{
					newChildren[i] = blendtree.children[i];
					newChildren[i].motion = GetCorresponingClip((AnimationClip)blendtree.children[i].motion);
				}
				blendtree.children = newChildren;
			}
			else
			{
				foreach (var child in blendtree.children)
				{
					ReplaceAnimationClips(child.motion as BlendTree);
				}
			}
		}

		AnimationClip GetCorresponingClip(AnimationClip clip)
		{
			string path = AssetDatabase.GetAssetPath(clip);
			var newPath = clipNameReplacementRules.Aggregate(path, (current, rule) => 
				                                                       current.Replace(rule.oldValue, rule.newValue));

			var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newPath);
			if (newClip != null)
			{
				Debug.LogFormat("Replaced clip {0}", newPath);
				return newClip;
			}
			// clip not found
			Debug.LogErrorFormat("No clip found at {0}", newPath);
			return useBaseClipsIfNotFound ? clip : null;
		}
	
		void OnGUI()
		{
			baseController = EditorGUILayout.ObjectField("Base controller", baseController, 
			                                             typeof(AnimatorController), false, null) as AnimatorController;

			newControllerName = EditorGUILayout.TextField("New controller name", newControllerName);
		
			// use property drawer to elegantly draw array
			SerializedObject so = new SerializedObject(this);
			SerializedProperty rulesProperty = so.FindProperty("clipNameReplacementRules");
			EditorGUILayout.PropertyField(rulesProperty, true);
			so.ApplyModifiedProperties();
		
			useBaseClipsIfNotFound = EditorGUILayout.Toggle("Use base clip if clip not found", useBaseClipsIfNotFound);
			
			if (baseController != null && GUILayout.Button("Replace clips"))
			{
				string pathToBase = AssetDatabase.GetAssetPath(baseController);
				string newAssetPath = pathToBase.Replace(baseController.name, newControllerName);
			
			var femaleController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newAssetPath);
			if (femaleController != null)
			{
				if (!EditorUtility.DisplayDialog("Overwrite file?",
				string.Format("A file with the name: {0} was found", newControllerName), "Overwrite", "Cancel"))
				{
					return;
				}
			}
			
			AssetDatabase.CopyAsset(pathToBase, newAssetPath);
			femaleController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newAssetPath);

			foreach (var stateMachine in femaleController.layers[0].stateMachine.stateMachines)
			{
				foreach (var states in stateMachine.stateMachine.states)
				{
					var clip = states.state.motion as AnimationClip;
					if (clip != null)
					{
						states.state.motion = GetCorresponingClip((AnimationClip)states.state.motion);
						continue;
					}
					ReplaceAnimationClips(states.state.motion as BlendTree);
				}
			}
			EditorUtility.SetDirty(femaleController);
			}
		}
	}
}
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace StandardAssets.TFBGames.Editor
{
	/// <summary>
	/// Editor window to replace animations clips in an AnimationController
	/// </summary>
	public class AnimationControllerEditor : EditorWindow
	{
		[Serializable]
		public struct ReplacementRule
		{
			public string oldValue,
			              newValue;
		}

		public ReplacementRule[] clipNameReplacementRules;

		AnimatorController baseController;
		bool useBaseClipsIfNotFound;
		string newControllerName = "NewAnimationController";

		[MenuItem("24 Bit Games/Animation Controller Clip Replacer")]
		public static void ShowWindow()
		{
			GetWindow(typeof(AnimationControllerEditor));
		}

		void ReplaceAnimationClips(BlendTree blendtree)
		{
			if ((blendtree == null) || (blendtree.children.Length == 0))
			{
				return;
			}

			if (blendtree.children[0].motion is AnimationClip)
			{
				int length = blendtree.children.Length;
				ChildMotion[] newChildren = new ChildMotion[length];
				for (int i = 0; i < length; i++)
				{
					newChildren[i] = blendtree.children[i];
					newChildren[i].motion = GetCorrespondingClip((AnimationClip) blendtree.children[i].motion);
				}

				blendtree.children = newChildren;
			}
			else
			{
				foreach (ChildMotion child in blendtree.children)
				{
					ReplaceAnimationClips(child.motion as BlendTree);
				}
			}
		}

		AnimationClip GetCorrespondingClip(AnimationClip clip)
		{
			string path = AssetDatabase.GetAssetPath(clip);
			string newPath = clipNameReplacementRules.Aggregate(path, (current, rule) =>
				                                                          current.Replace(rule.oldValue, rule.newValue));

			var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newPath);
			if (newClip != null)
			{
				Debug.LogFormat("Found replacement clip, {0}, for {1}", newPath, path);
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
			var so = new SerializedObject(this);
			SerializedProperty rulesProperty = so.FindProperty("clipNameReplacementRules");
			EditorGUILayout.PropertyField(rulesProperty, true);
			so.ApplyModifiedProperties();

			useBaseClipsIfNotFound = EditorGUILayout.Toggle("Use base clip if clip not found", useBaseClipsIfNotFound);

			if ((baseController != null) && GUILayout.Button("Replace clips"))
			{
				string pathToBase = AssetDatabase.GetAssetPath(baseController);
				string newAssetPath = pathToBase.Replace(baseController.name, newControllerName);

				var newController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newAssetPath);
				if (newController != null)
				{
					if (!EditorUtility.DisplayDialog("Overwrite file?",
					                                 string.Format("A file with the name: {0} was found", newControllerName),
					                                 "Overwrite", "Cancel"))
					{
						return;
					}
				}

				AssetDatabase.CopyAsset(pathToBase, newAssetPath);
				newController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newAssetPath);

				foreach (AnimatorControllerLayer layer in newController.layers)
				{
					TraverseStatemachineToReplaceClips(layer.stateMachine);
				}
			}
		}

		void TraverseStatemachineToReplaceClips(AnimatorStateMachine stateMachine)
		{
			foreach (ChildAnimatorState childState in stateMachine.states)
			{
				var clip = childState.state.motion as AnimationClip;
				if (clip != null)
				{
					childState.state.motion = GetCorrespondingClip(clip);
					continue;
				}

				// not a clip, must be a blend tree
				ReplaceAnimationClips(childState.state.motion as BlendTree);
			}

			foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
			{
				TraverseStatemachineToReplaceClips(childStateMachine.stateMachine);
			}
		}
	}
}
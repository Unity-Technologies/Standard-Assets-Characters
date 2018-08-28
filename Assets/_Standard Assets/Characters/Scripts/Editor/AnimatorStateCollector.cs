using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor
{
	public static class AnimatorStateCollector
	{
		private static List<string> stateNames;
		private static bool prefixes,
		             blendTrees;

		public static List<string> CollectStatesNames(AnimatorController ac, int layerIndex, bool includePrefixes = false,
		                          bool includeBlendTrees = true)
		{
			prefixes = includePrefixes;
			blendTrees = includeBlendTrees;

			stateNames = new List<string> {"(default)"};

			if (ac != null && layerIndex >= 0 && layerIndex < ac.layers.Length)
			{
				AnimatorStateMachine fsm = ac.layers[layerIndex].stateMachine;
				CollectStatesFromFsm(fsm, string.Empty);
			}

			return stateNames;
		}

		private static void CollectStatesFromFsm(AnimatorStateMachine fsm, string displayPrefix)
		{
			ChildAnimatorState[] fsmStates = fsm.states;
			foreach (ChildAnimatorState childAnimatorState in fsmStates)
			{
				AnimatorState state = childAnimatorState.state;
				string displayName = prefixes ? displayPrefix + state.name : state.name;
				AddState(displayName);

				// Also process clips as pseudo-states, if more than 1 is present.
				// Since they don't have hashes, we can manufacture some.
				List<string> clips = CollectClipNames(state.motion);
				if (clips.Count > 1)
				{
					string subStatePrefix = displayPrefix + state.name + ".";
					foreach (string name in clips)
					{
						displayName = prefixes ? subStatePrefix + name : name;
						AddState(displayName);
					}
				}
			}

			ChildAnimatorStateMachine[] fsmChildren = fsm.stateMachines;
			foreach (ChildAnimatorStateMachine child in fsmChildren)
			{
				string displayName = displayPrefix + child.stateMachine.name;
				string adjustedDisplayName = prefixes ? displayName + "." : displayName;
				CollectStatesFromFsm(child.stateMachine, adjustedDisplayName);
			}
		}

		private static List<string> CollectClipNames(Motion motion)
		{
			List<string> names = new List<string>();
			var clip = motion as AnimationClip;
			if (clip != null)
			{
				names.Add(clip.name);
			}

			var tree = motion as BlendTree;
			if (tree == null || !blendTrees)
			{
				return names;
			}

			ChildMotion[] children = tree.children;
			foreach (ChildMotion child in children)
			{
				names.AddRange(CollectClipNames(child.motion));
			}
			return names;
		}

		private static void AddState(string displayName)
		{
			stateNames.Add(displayName);
		}
	}
}
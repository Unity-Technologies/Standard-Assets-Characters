using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TFBGames.Editor
{
	public class AnimationControllerTool : EditorWindow
	{ 
		private struct AnimationControllerToolConfig
		{
			public string newName;

			public MissingClipRule missingClipRule;

			public bool solveInvalidReferences;
			public bool autoCreateMissingFile;
		}

		[Serializable]
		private struct Rule
		{
			public string
				find,
				replace;

	 
		}

		private struct ClipResult
		{
			public Object Object;
			public MessageType type;
			public string message;
		}

		private enum MissingClipRule
		{
			UseEmpty,
			UseSource
		}

		private enum EditorMode
		{
			Rules,
			Paths,
			Results
		}

		// only needs to be init once
		private static string[] s_ModeLabels;
		private static GUIContent s_RulesTitle;

		/// <summary>
		/// Serialized current window.
		/// </summary>
		private static SerializedObject s_SerializedWindow;

		[SerializeField]
		private AnimationControllerToolConfig config;

		[SerializeField]
		private Rule[] rules;

		[SerializeField]
		private Vector2 scroll;

		private AnimatorController
			selectedController,
			// the last processed results for dump.
			lastSourceController,
			lastResultController;

		/// <summary>
		/// Serialized selected <see cref="AnimatorController" />
		/// </summary>
		private SerializedObject serializedObject;

		/// <summary>
		/// The current rules property.
		/// </summary>
		private SerializedProperty rulesProperty;

		private EditorMode mode;

		private readonly List<ClipResult> lastResults = new List<ClipResult>();

		[MenuItem("24 Bit Games/Animation Duplication Tool")]
		public static AnimationControllerTool ShowWindow()
		{
			var window = GetWindow<AnimationControllerTool>();
			// only used to treat Rules array as serialized property array.
			s_SerializedWindow = new SerializedObject(window);
			return window;
		}

		private void OnGUI()
		{
			scroll = EditorGUILayout.BeginScrollView(scroll);
			scroll.x = 0;

			if (s_RulesTitle == null)
			{
				s_RulesTitle = new GUIContent("Find/Replace Rules");
			}

			if (s_SerializedWindow == null)
			{
				s_SerializedWindow = new SerializedObject(this);
			}

			rulesProperty = s_SerializedWindow.FindProperty("rules");

			DrawSetup();

			bool isValid = (selectedController != null) && DrawValidation();

			if (selectedController != null)
			{
				EditorGUILayout.Space();

				// helper to wrap a block of content in a "RL" prefixed GUI Panel.
				s_RulesTitle.text = mode.ToString();
				TBFEditorStyles.DrawPanel(s_RulesTitle, DrawMainPanel);

				s_SerializedWindow.ApplyModifiedProperties();

				if (serializedObject != null)
				{
					serializedObject.ApplyModifiedProperties();
				}

				if (!isValid)
				{
					EditorGUILayout.HelpBox("There are unresolved issues.", MessageType.Error);
				}

				EditorGUI.BeginDisabledGroup(!isValid);
				{
					if (GUILayout.Button("Execute"))
					{
						// TEMPORARY:
						Debug.Log("Begin Animation State Copy");
						Process(selectedController);
						if (config.autoCreateMissingFile)
						{
							DumpResults();
						}

						mode = EditorMode.Results;
					}
				}

				EditorGUI.EndDisabledGroup();
			}

			EditorGUILayout.EndScrollView();
		}

		#region Process
		/// <summary>
		/// Entry point for processing the entire controller graph.
		/// </summary>
		private void Process(AnimatorController src)
		{
			lastResults.Clear();

			string name = config.newName;

			string
				// destination:
				srcPath = AssetDatabase.GetAssetPath(src),
				dstPath = ApplyRulesToString(srcPath.Replace(selectedController.name, name));

			var file = new FileInfo(dstPath);
			if (!file.Directory.Exists)
			{
				file.Directory.Create();
			}

			AssetDatabase.Refresh();

			AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dstPath);
			var dst = AssetDatabase.LoadAssetAtPath<AnimatorController>(dstPath);
			lastResultController = dst;
			lastSourceController = src;

			// Passing both the original and new controller so we can compare motion states later
			// so we can handle missing states in the new controller for those defined in original
			// (only when clip is found in replacement path, as they would be a perfect match otherwise).
			int i = 0;
			AnimatorControllerLayer[] dstLayers = dst.layers;
			//AnimatorControllerLayer[] srcLayers = src.layers;
			while (i < dstLayers.Length)
			{
				TraverseStates(dstLayers[i].stateMachine);
				i++;
			}
		}

		private void TraverseStates(AnimatorStateMachine dstMachine)
		{
			if (dstMachine == null)
			{
				return;
			}

			ChildAnimatorState[] dstStates = dstMachine.states;

			if ((dstStates.Length < 1) && (dstMachine.stateMachines.Length < 1))
			{
				AddResult(string.Format("State Machine \"{0}\" is empty", dstMachine.name), MessageType.Warning);
			}

			ChildAnimatorState dstState;
			for (int i = 0; i < dstStates.Length; i++)
			{
				dstState = dstStates[i];
				var clip = dstState.state.motion as AnimationClip;
				if (clip != null)
				{
					// animation clip in root? 
					// original implementation did nothing here (shouldnt this also be attempting to replace clip?). 
					dstState.state.motion = TryGetReplacementClip(clip, dstState.state);
					continue;
				}

				TraverseClips(dstState.state.motion as BlendTree);
			}

			ChildAnimatorStateMachine[] subStates = dstMachine.stateMachines;
			for (int i = 0; i < subStates.Length; i++)
			{
				TraverseStates(subStates[i].stateMachine);
			}
		}

		private void TraverseClips(BlendTree dstTree)
		{
			if ((dstTree == null) || (dstTree.children.Length < 1))
			{
				return;
			}

			ChildMotion[] dstChildren = dstTree.children;

			AnimationClip clip;

			// Let user know of empty blend trees..
			if (dstChildren.Length < 1)
			{
				AddResult(string.Format("BlendTree \"{0}\" is empty.", dstTree.name), MessageType.Error);
			}

			for (int i = 0; i < dstChildren.Length; i++)
			{
				ChildMotion dst = dstChildren[i];

				if (dst.motion is AnimationClip)
				{
					clip = TryGetReplacementClip(dst.motion as AnimationClip, dstTree);

					dst.motion = clip;
					dstChildren[i] = dst; // because struct, 
				}
				else
				{
					// blend tree has seeds
					TraverseClips(dst.motion as BlendTree);
				}
			}

			dstTree.children = dstChildren;
		}

		/// <summary>
		/// Method where we try to find replacement clip and handle missing cases.
		/// Second paramater is for logging to identify where a clip is assigned in a graph.
		/// </summary>
		private AnimationClip TryGetReplacementClip(AnimationClip clip, Object parent)
		{
			// errr.... NOTE: GetAssetAtPath with a Asset/SubAsset object (EG: Model) will likely return only the first animation clip in a multi asset asset.
			// We will use CheckCLipAssetFile to check this...

			string path = AssetDatabase.GetAssetPath(clip);
			path = ApplyRulesToString(path);
			var newclip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

			if (newclip != null)
			{
				// clip in new path exists.
				Debug.LogFormat("Found replacement clip: {0} for {1}", newclip.name, clip.name);

				// compare model asset for clip:
				CheckClipAssetFile(clip, ref newclip);
			}
			else
			{
				if (clip != null)
				{
					AddResult(string.Format("Clip \"{0}\" is missing at \"{1}\".", clip.name, path),
					          MessageType.Warning, clip);
				}
				else if (parent != null)
				{
					AddResult(string.Format("Clip in \"{0}\" is null.", parent.name), MessageType.Error,
					          selectedController);
				}
			}

			if ((config.missingClipRule == MissingClipRule.UseSource) && (newclip == null))
			{
				newclip = clip;
			}

			return newclip;
		}

		// Checks animation clips in target asset to verify we actually have a clip by it's name, 
		// with the side effect of also being able to check if we have the wrong clip (probably).
		private void CheckClipAssetFile(AnimationClip src, ref AnimationClip target, Motion parent = null)
		{
			string srcPath = AssetDatabase.GetAssetPath(src);
			string tgtPath = ApplyRulesToString(srcPath); //AssetDatabase.GetAssetPath(target);

			// applies if animation names contain strings modified by rules, so we also have a fallback below to also check for original name
			// as a file might have the right name, but the clips inside it might not contain expected "new" names, but rather the original name.
			string expectedName = ApplyRulesToString(src.name);

			AnimationClip matched;

			Object[]
				tgtObjects = AssetDatabase.LoadAllAssetsAtPath(tgtPath);

			// one time comparison of all animation clips.
			if (ContainsObject(expectedName, tgtObjects, out matched))
			{
				AddResult(
					string.Format("Match Success \"{0}\" for \"{1}\" in \"{2}\"", expectedName, src.name, tgtPath),
					MessageType.Info, matched);
			}
			else if (ContainsObject(src.name, tgtObjects, out matched))
			{
				AddResult(
					string.Format("Match Success [Original Name] \"{0}\" for \"{1}\" in \"{2}\"", expectedName,
					              src.name, tgtPath), MessageType.Info, matched);
			}
			else
			{
				AddResult(
					string.Format("Expected Animationnot found: \"{0}\" for \"{1}\" in \"{2}\"", expectedName, src.name,
					              srcPath), MessageType.Info, src);
			}

			if ((matched != null) && (matched.name != target.name))
			{
				AddResult(
					string.Format("Possible Incorrect Reference: \"{0}\" for \"{1}\", Expected \"{3}\", in \"{2}\"",
					              target.name, src.name, parent == null ? srcPath : parent.name, matched.name),
					MessageType.Warning, target);

				// this might be the way to automatically replace incorrect clips gathered from GetAssetAtPath 
				// in multi clip assets where teh first clip is always returned.
				// commented out for now as I am not sure if it will break non-broken results yet however:
				if (config.solveInvalidReferences)
				{
					target = matched;
					AddResult("Replaced previous clip reference with matched reference.", MessageType.None, matched);
				}
			}
		}

		// check if asset of type with name exists in the array.
		private bool ContainsObject<T>(string name, Object[] inArray, out T result) where T : Object
		{
			for (int i = 0; i < inArray.Length; i++)
			{
				Object current = inArray[i];
				var variable = current as T;
				if (variable != null && (variable.name == name))
				{
					// TODO: should probably check case-insensitive names in event of a typo.
					result = variable;
					return true;
				}
			}

			result = null;
			return false;
		}

		private string ApplyRulesToString(string str)
		{
			int i = 0;
			if (rules == null)
			{
				return str;
			}

			while (i < rules.Length)
			{
				// Note: incrementor. Don't touch my cookies.
				Rule current = rules[i++];

				if (!string.IsNullOrEmpty(current.find.Trim()))
				{
					str = str.Replace(current.find, current.replace);
				}
			}

			return str;
		}

		private void AddResult(string message, MessageType type, Object context = null)
		{
			lastResults.Add(new ClipResult {message = message, type = type, Object = context});
		}

		private void DumpResults()
		{
			if ((lastSourceController == null) || ((lastResultController == null) && (lastResults.Count > 0)))
			{
				return;
			}

			string dumpFile = AssetDatabase.GetAssetPath(lastResultController).Replace(".controller", "_Missing.txt");

			List<string> result = new List<string>();

			result.Add("Animation Controller Duplicator Tool Result");
			result.Add(string.Format("Source Controller: {0}", lastSourceController.name));
			result.Add(string.Format("Output Controller: {0}", lastResultController.name));
			result.Add(string.Empty);

			for (int i = 0; i < lastResults.Count; i++)
			{
				ClipResult curr = lastResults[i];
				result.Add(curr.message);
			}

			// for now just dumps to the same location as generated controller:
			File.WriteAllText(dumpFile, string.Join("\r\n", result.ToArray()));
		}
		#endregion

		#region Layout
		/*
		 *  All GUI layout calls are prefixed with "Draw"
		 */

		private void DrawMainPanel()
		{
			mode = (EditorMode) GUILayout.Toolbar((int) mode, Enum.GetNames(typeof(EditorMode)));

			switch (mode)
			{
				default:
				case EditorMode.Rules:
					DrawRules();
					break;

				case EditorMode.Paths:
					DrawPaths();
					break;

				case EditorMode.Results:
					DrawResults();
					break;
			}
		}

		// draw asset paths.
		private void DrawPaths()
		{
			if (selectedController)
			{
				string 	path = AssetDatabase.GetAssetPath(selectedController),
						newPath = ApplyRulesToString(path);

				EditorGUI.BeginDisabledGroup(true);

				EditorGUILayout.LabelField("Src Path");
				EditorGUILayout.TextField(path);

				EditorGUILayout.LabelField("Dst Path");
				EditorGUILayout.TextField(newPath);

				EditorGUI.EndDisabledGroup();
				EditorGUILayout.Space();
			}
		}

		private void DrawResults()
		{
			EditorGUILayout.LabelField("Messages are results from the last execution.");
			if (lastResults.Count < 1)
			{
				EditorGUILayout.HelpBox("Everything is just peachy.", MessageType.None);
			}
			else
			{
				if (GUILayout.Button("Dump to file"))
				{
					DumpResults();
				}

				ClipResult curr;
				for (int i = 0; i < lastResults.Count; i++)
				{
					curr = lastResults[i];

					Color color;
					switch (curr.type)
					{
						default:
							color = new Color(0, 1, 1, 0.1f);
							break;

						case MessageType.Error:
							color = new Color(1, 0, 0, 0.1f);
							break;

						case MessageType.Warning:
							color = new Color(1, 1, 0, 0.1f);
							break;

						case MessageType.Info:
							color = new Color(0, 1, 0, 0.1f);
							break;
					}

					bool isAsset = (curr.Object != null) && AssetDatabase.Contains(curr.Object);

					EditorGUILayout.BeginHorizontal("box");
					EditorGUILayout.LabelField(curr.message);
					if (isAsset && GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
					{
						Selection.activeObject = curr.Object;
					}

					EditorGUILayout.EndHorizontal();
					if (curr.type != MessageType.None)
					{
						Rect rect = GUILayoutUtility.GetLastRect();
						EditorGUI.DrawRect(rect, color);
					}
				}
			}
		}

		private bool DrawValidation()
		{
			bool result = true;

			if (selectedController == null)
			{
				EditorGUILayout.HelpBox("Controller is empty", MessageType.Warning);
				result = false;
			}

			if (selectedController != null)
			{
				if (selectedController.layers.Length < 1)
				{
					EditorGUILayout.HelpBox("Selected controller has no layers", MessageType.Error);
					result = false;
				}
			}

			if (!DrawValidateRules())
			{
				result = false;
			}

			return result;
		}

		private bool DrawValidateRules()
		{
			int i = 0;
			while (i < rulesProperty.arraySize)
			{
				SerializedProperty curr = rulesProperty.GetArrayElementAtIndex(i++);
				if (string.IsNullOrEmpty(curr.FindPropertyRelative("find").stringValue) ||
				    string.IsNullOrEmpty(curr.FindPropertyRelative("find").stringValue))
				{
					EditorGUILayout.HelpBox("A Rule cannot have an empty field.", MessageType.Error);
					return false;
				}
			}

			return true;
		}

		private void DrawSetup()
		{
			bool changed = false;
			EditorGUI.BeginChangeCheck();
			selectedController = (AnimatorController) 
				EditorGUILayout.ObjectField("Source", selectedController,typeof(AnimatorController), false);

			if (EditorGUI.EndChangeCheck())
			{
				changed = true;
			}

			// setup serialized object if required.
			if (selectedController == null)
			{
				serializedObject = null;
			}
			else if (changed || (serializedObject == null))
			{
				serializedObject = new SerializedObject(selectedController);
			}

			if (selectedController != null)
			{
				if ((config.newName == null) || string.IsNullOrEmpty(config.newName.Trim()))
				{
					config.newName = ApplyRulesToString(selectedController.name);
				}
			}

			config.newName = EditorGUILayout.TextField("New Name", config.newName);

			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Relative paths are supported.", (GUIStyle) "minilabel");
			EditorGUI.indentLevel--;

			config.missingClipRule =
				(MissingClipRule) EditorGUILayout.EnumPopup("Missing Animations", config.missingClipRule);

			// In multi clip files, try to replace incorrect loaded clip with matched clip when FBX contains a
			//  clip with expected name. 
			config.solveInvalidReferences =
				EditorGUILayout.Toggle("Solve Invalid Clip References", config.solveInvalidReferences);
			config.autoCreateMissingFile =
				EditorGUILayout.Toggle("Auto Create Error File", config.autoCreateMissingFile);
		}

		private void DrawRules()
		{
			SerializedProperty current,
			                   find,
			                   replace;

			bool canDelete = rulesProperty.arraySize > 1;

			if (rulesProperty.arraySize < 1)
			{
				rulesProperty.arraySize++;
			}

			for (int i = 0; i < rulesProperty.arraySize; )
			{
				bool deleted = false;
				// note: incrementor otherwise infinate:
				current = rulesProperty.GetArrayElementAtIndex(i);

				find = current.FindPropertyRelative("find");
				replace = current.FindPropertyRelative("replace");

				// outer container for delete button without breaking label alignement.
				EditorGUILayout.BeginVertical();
				{
					EditorGUILayout.BeginHorizontal();
					find.stringValue = EditorGUILayout.TextField(find.stringValue).Trim();
					replace.stringValue = EditorGUILayout.TextField(replace.stringValue).Trim();
					EditorGUI.BeginDisabledGroup(!canDelete);
					if (GUILayout.Button("X", TBFEditorStyles.DeletArrayItemButton))
					{
						s_SerializedWindow.ApplyModifiedProperties();
						s_SerializedWindow.UpdateIfRequiredOrScript();
						rulesProperty.DeleteArrayElementAtIndex(i);
						deleted = true;
					}

					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();

				if (string.IsNullOrEmpty(find.stringValue.Trim()) || string.IsNullOrEmpty(replace.stringValue.Trim()))
				{
					Rect rect = GUILayoutUtility.GetLastRect();
					EditorGUI.DrawRect(rect, new Color(1, 0, 0, 0.25f));
				}

				if (!deleted)
				{
					i++;
				}
			}

			if (GUILayout.Button("Add Rule", "minibutton"))
			{
				rulesProperty.arraySize++;
				ClearLastRule(rulesProperty);
			}
		}

		/// <summary>
		/// Adding an array element to a serialized property via <see cref="SerializedProperty.arraySize" />
		/// does not take affect immediatly if you want to set a newly added array element's values right away.
		/// Adding an item to array always duplicates the previous element.
		/// This allows to to set the (newest) element to default/empty and accessable immedietly.
		/// </summary>
		private void ClearLastRule(SerializedProperty rules)
		{
			serializedObject.ApplyModifiedProperties();
			serializedObject.UpdateIfRequiredOrScript();

			SerializedProperty lastItem = rules.GetArrayElementAtIndex(rules.arraySize - 1);
			lastItem.FindPropertyRelative("find").stringValue = string.Empty;
			lastItem.FindPropertyRelative("replace").stringValue = string.Empty;
		}
		#endregion
	}
}
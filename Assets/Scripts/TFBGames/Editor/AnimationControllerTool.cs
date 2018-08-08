using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using Object = UnityEngine.Object;

namespace TFBGames.Editor
{
    public class AnimationControllerTool : EditorWindow
    {
        public AnimationControllerToolConfig Config;

        public Rule[] Rules;

        static Vector2 scroll;

 
        static string[] ModeLabels;

        AnimatorController
            selectedController,
            // the last processed results for dump.
            lastSourceController,
            lastResultController;

        /// <summary>
        /// Serialized selected <see cref="AnimatorController"/>
        /// </summary>
        SerializedObject
            serializedObject;

        /// <summary>
        /// The current rules property.
        /// </summary>
        SerializedProperty
            rulesProperty;

        /// <summary>
        /// Serialized current window.
        /// </summary>
        static SerializedObject
            serializedWindow;

 

        static GUIContent rulesTitle;

        EditorMode mode;

        List<ClipResult> lastResults = new List<ClipResult>();

  

        [MenuItem("24 Bit Games/Animation Duplication Tool")]
        public static AnimationControllerTool ShowWindow()
        {
            AnimationControllerTool window = GetWindow<AnimationControllerTool>();
            // only used to treat Rules array as serialized property array.
            serializedWindow = new SerializedObject(window);
            return window;
        }


        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            scroll.x = 0;

            if (rulesTitle == null)
                rulesTitle = new GUIContent("Find/Replace Rules");

            if (serializedWindow == null)
                serializedWindow = new SerializedObject(this);
 
            rulesProperty = serializedWindow.FindProperty("Rules");
 

            DrawSetup();
            
            bool isValid = selectedController != null && DrawValidation();

            if (selectedController != null)
            {
                EditorGUILayout.Space();


                // helper to wrap a block of content in a "RL" prefixed GUI Panel.
                rulesTitle.text = mode.ToString();
                TBFEditorStyles.DrawPanel(rulesTitle, DrawMainPanel);

                serializedWindow.ApplyModifiedProperties();

                if (serializedObject != null)
                    serializedObject.ApplyModifiedProperties();

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
                        if (Config.AutoCreateMissingFile)
                            DumpResults();
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
        void Process(AnimatorController src)
        {
            lastResults.Clear();
 
            string name = Config.NewName;

            string
                // destination:
                srcPath = AssetDatabase.GetAssetPath(src), 
                dstPath = ApplyRulesToString(srcPath.Replace(selectedController.name, name));

            FileInfo file = new FileInfo(dstPath);
            if (!file.Directory.Exists)
                file.Directory.Create();

            AssetDatabase.Refresh();

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(src), dstPath); 
            AnimatorController dst = AssetDatabase.LoadAssetAtPath<AnimatorController>(dstPath);
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
                TraverseStates( dstLayers[i].stateMachine);
                i++;
            }
        }




 

        void TraverseStates( AnimatorStateMachine dstMachine)
        {
  
            if(dstMachine == null)
            {
                return;
            }
            int i = 0;
            ChildAnimatorState[] dstStates = dstMachine.states;
           
            ChildAnimatorState dstState;

            if(dstStates.Length < 1 && dstMachine.stateMachines.Length < 1)
            {
                AddResult(string.Format("State Machine \"{0}\" is empty", dstMachine.name), MessageType.Warning);
            }
  
            while(i < dstStates.Length)
            {
                dstState = dstStates[i];
                i++;

                var clip = dstState.state.motion as AnimationClip; 
                if(clip != null)
                {
                    // animation clip in root? 
                    // original implementation did nothing here (shouldnt this also be attempting to replace clip?). 
                    dstState.state.motion = TryGetReplacementClip(clip, dstState.state);
                    continue;
                }



                TraverseClips( dstState.state.motion as BlendTree); 
            }

            i = 0;
            var subStates = dstMachine.stateMachines;
            while( i < subStates.Length)
            {
                TraverseStates( subStates[i++].stateMachine );
            }
        }





        void TraverseClips( BlendTree dstTree)
        {
            // TODO: we need to pass original blend tree counterpart for comparison as well?

            if (dstTree == null || dstTree.children.Length < 1)
            {
                return;
            }

            int i = 0;
            ChildMotion[] dstChildren = dstTree.children;

            AnimationClip clip;

            // Let user know of empty blend trees..
            if(dstChildren.Length < 1)
            {
                AddResult(string.Format("BlendTree \"{0}\" is empty.", dstTree.name), MessageType.Error);
            }
 
            while(i < dstChildren.Length)
            {
                ChildMotion dst = dstChildren[i];
 
                if(dst.motion is AnimationClip)
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

                i++;
            } 
            dstTree.children = dstChildren; 
        }



        /// <summary>
        /// Method where we try to find replacement clip and handle missing cases.
        /// Second paramater is for logging to identify where a clip is assigned in a graph.
        /// </summary> 
        AnimationClip TryGetReplacementClip(AnimationClip clip, Object parent)
        {
            // errr.... NOTE: GetAssetAtPath with a Asset/SubAsset object (EG: Model) will likely return only the first animation clip in a multi asset asset.
            // We will use CheckCLipAssetFile to check this...

            string path = AssetDatabase.GetAssetPath(clip);
            path = ApplyRulesToString(path);
            AnimationClip newclip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (newclip != null)
            {
                // clip in new path exists.
                Debug.LogFormat("Found replacement clip: {0} for {1}", newclip.name, clip.name);

                // compare model asset for clip:
                CheckClipAssetFile(clip, ref newclip);
            }
            else
            {
                if(clip != null)
                {
                    AddResult(string.Format("Clip \"{0}\" is missing at \"{1}\".", clip.name, path), MessageType.Warning, clip);   
                }
                else
                {
                    if(parent != null)
                        AddResult(string.Format("Clip in \"{0}\" is null.", parent.name), MessageType.Error, selectedController);
                }
            }

            switch(Config.MissingClipRule)
            {
                //default: 
                //case MissingClipRule.UseEmpty: 
                    //Debug.Log("Did not find replacement clip. Using empty state.");
                    //break; 

                case MissingClipRule.UseSource:
                    if (newclip == null) newclip = clip;
                    //Debug.LogFormat("Did not find replacement clip. Fallback to: \"{0}\"", clip == null     ? "none" : clip.name);
                    break;
            }
            return newclip;
        }



        // Checks animation clips in target asset to verify we actually have a clip by it's name, 
        // with the side effect of also being able to check if we have the wrong clip (probably).
        void CheckClipAssetFile(AnimationClip src, ref AnimationClip target, Motion parent = null)
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
            if(ContainsObject(expectedName, tgtObjects, out matched))
            {
                AddResult(string.Format("Match Success \"{0}\" for \"{1}\" in \"{2}\"", expectedName, src.name, tgtPath), MessageType.Info, matched);

            }
            else if (ContainsObject(src.name, tgtObjects, out matched))
            {
                // we found a clip using the original name, instead of expected name, from rules in target asset
                AddResult(string.Format("Match Success [Original Name] \"{0}\" for \"{1}\" in \"{2}\"", expectedName, src.name, tgtPath), MessageType.Info, matched);
            }
            else
            { 
                // target asset does not contain animation clip.
                AddResult(string.Format("Expected Animationnot found: \"{0}\" for \"{1}\" in \"{2}\"", expectedName, src.name, srcPath), MessageType.Info, src);
            }

            if(matched != null && matched.name != target.name) 
            {
                
                AddResult(string.Format("Possible Incorrect Reference: \"{0}\" for \"{1}\", Expected \"{3}\", in \"{2}\"", target.name, src.name, parent == null ? srcPath : parent.name, matched.name), MessageType.Warning, target);
              
                // this might be the way to automatically replace incorrect clips gathered from GetAssetAtPath 
                // in multi clip assets where teh first clip is always returned.
                // commented out for now as I am not sure if it will break non-broken results yet however:
                if(Config.SolveInvalidReferences)
                {
                    target = matched;
                    AddResult("Replaced previous clip reference with matched reference.", MessageType.None, matched);
                }
            } 
        }

       
        // check if asset of type with name exists in the array.
        bool ContainsObject<T>(string name, Object[] inArray, out T result) where T : Object
        { 
            for (int i = 0; i < inArray.Length; i++)
            {
                Object current = inArray[i];
                if(current is T && current.name == name)
                {
                    // TODO: should probably check case-insensitive names in event of a typo.
                    result = (T)current;
                    return true;
                }
            }
            result = null;
            return false;
        }

        string ApplyRulesToString(string str)
        {
            int i = 0;
            if (Rules == null) return str;
            while(i < Rules.Length)
            {
                // Note: incrementor. Don't touch my cookies.
                Rule current = Rules[i++]; 

                if(!string.IsNullOrEmpty(current.Find.Trim()))
                {
                    str = str.Replace(current.Find, current.Replace);
                }
            }
            return str;
        }


        void AddResult(String message, MessageType type, Object context = null)
        {
            lastResults.Add(new ClipResult() { Message = message, Type = type, Object = context });
        }

        void DumpResults()
        {
            if(lastSourceController == null || (lastResultController == null && lastResults.Count > 0))
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
                result.Add(curr.Message);
            }  

            // for now just dumps to the same location as generated controller:
            File.WriteAllText(dumpFile, string.Join("\r\n",result.ToArray()));
        }




        #endregion



        #region Layout
        /*
         *  All GUI layout calls are prefixed with "Draw"
         */

        void DrawMainPanel()
        {
            mode = (EditorMode)GUILayout.Toolbar((int)mode, Enum.GetNames(typeof(EditorMode)));

            switch(mode)
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
        void DrawPaths()
        {
            if(selectedController)
            {
                string
                    //name    = selectedController.name,
                    path    = AssetDatabase.GetAssetPath(selectedController),
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

        void DrawResults()
        {
            EditorGUILayout.LabelField("Messages are results from the last execution.");
            if(lastResults.Count < 1)
            {
                EditorGUILayout.HelpBox("Everything is just peachy.", MessageType.None);
            }
            else
            {
                if(GUILayout.Button("Dump to file"))
                {
                    DumpResults();
                }


                ClipResult curr;
                for (int i = 0; i < lastResults.Count; i++)
                {
                    curr = lastResults[i];

                    Color color; 
                    switch(curr.Type)
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

                    bool isAsset = curr.Object != null && AssetDatabase.Contains(curr.Object);

                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(curr.Message);
                    if(isAsset && GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        Selection.activeObject = curr.Object;
                    } 
                    EditorGUILayout.EndHorizontal();
                    if(curr.Type != MessageType.None)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        EditorGUI.DrawRect(rect, color);
                    } 
                }
            }
        }
 

        bool DrawValidation()
        {
            bool result = true;

            if( selectedController == null)
            { 
                EditorGUILayout.HelpBox("Controller is empty", MessageType.Warning);
                result = false;
            }

            if(selectedController != null)
            {
                // if there are no layers, then there are no states / state machines/ anything...
                if(selectedController.layers.Length < 1)
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



        bool DrawValidateRules()
        {
            int i = 0;
            while(i < rulesProperty.arraySize)
            {
                SerializedProperty curr = rulesProperty.GetArrayElementAtIndex(i++);
                if(string.IsNullOrEmpty( curr.FindPropertyRelative("Find").stringValue) || string.IsNullOrEmpty( curr.FindPropertyRelative("Find").stringValue))
                {
                    EditorGUILayout.HelpBox("A Rule cannot have an empty field.", MessageType.Error);
                    return false;
                }
            }
            return true;
        }
   




        void DrawSetup()
        {
            bool changed = false;
            EditorGUI.BeginChangeCheck();
            selectedController = (AnimatorController)EditorGUILayout.ObjectField("Source", selectedController, typeof(AnimatorController), false);
            //if (selectedController == null)
            //{
                //EditorGUILayout.HelpBox("No Controller Selected", MessageType.Warning);
                //return;
            //}
            // only apply new serialized object if field has changed.
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }

            // setup serialized object if required.
            if (selectedController == null)
            {
                serializedObject = null;
            }
            else if (changed || serializedObject == null)
            {
                serializedObject = new SerializedObject(selectedController);
            }

            if(selectedController != null)
            {
                if(Config.NewName == null || string.IsNullOrEmpty(Config.NewName.Trim()))
                {
                    Config.NewName = ApplyRulesToString(selectedController.name);
                }
            }

            Config.NewName          = EditorGUILayout.TextField("New Name", Config.NewName);

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Relative paths are supported.", (GUIStyle)"minilabel");
            EditorGUI.indentLevel--;

            Config.MissingClipRule = (MissingClipRule) EditorGUILayout.EnumPopup("Missing Animations", Config.MissingClipRule);

            // In multi clip files, try to replace incorrect loaded clip with matched clip when FBX contains a clip with expected
            // name. Hopefully not replacing it with incorrect clip.
            Config.SolveInvalidReferences = EditorGUILayout.Toggle("Solve Invalid Clip References", Config.SolveInvalidReferences);
            Config.AutoCreateMissingFile = EditorGUILayout.Toggle("Auto Create Error File", Config.AutoCreateMissingFile);
        }



 
 


        void DrawRules()
        {
            int i = 0;
            SerializedProperty
                current,
                find,
                replace;

            bool
                canDelete = rulesProperty.arraySize > 1;
            
            if (rulesProperty.arraySize < 1)
            {
                rulesProperty.arraySize++;
            }

            while (i < rulesProperty.arraySize)
            {
                bool deleted = false;
                // note: incrementor otherwise infinate:
                current = rulesProperty.GetArrayElementAtIndex(i);

                find    = current.FindPropertyRelative("Find");
                replace = current.FindPropertyRelative("Replace");

                // outer container for delete button without breaking label alignement.
                EditorGUILayout.BeginVertical();
                {
                     
                    EditorGUILayout.BeginHorizontal();
                    find        .stringValue = EditorGUILayout.TextField(find.stringValue).Trim();
                    replace     .stringValue = EditorGUILayout.TextField(replace.stringValue).Trim(); 
                    EditorGUI.BeginDisabledGroup(!canDelete);
                    if(GUILayout.Button("X", TBFEditorStyles.DeletArrayItemButton))
                    {
                        serializedWindow.ApplyModifiedProperties();
                        serializedWindow.UpdateIfRequiredOrScript();
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
        /// Adding an array element to a serialized property via <see cref="SerializedProperty.arraySize"/> 
        /// does not take affect immediatly if you want to set a newly added array element's values right away.
        /// Adding an item to array always duplicates the previous element.
        /// This allows to to set the (newest) element to default/empty and accessable immedietly.
        /// </summary>
        void ClearLastRule(SerializedProperty rules)
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();

            SerializedProperty lastItem = rules.GetArrayElementAtIndex(rules.arraySize - 1);
            lastItem.FindPropertyRelative("Find").stringValue = string.Empty;
            lastItem.FindPropertyRelative("Replace").stringValue = string.Empty;
        }


        #endregion











        [Serializable]
        public struct AnimationControllerToolConfig
        {
            public string
                NewName;

            public MissingClipRule
                MissingClipRule;

            public bool SolveInvalidReferences;
            public bool AutoCreateMissingFile;
        }


        [Serializable]
        public struct Rule
        {
            public String
                Find,
                Replace;
        }
        struct ClipResult
        {
            public UnityEngine.Object Object;
            public MessageType Type;
            public String Message;
        }


        public enum MissingClipRule
        {
            UseEmpty,
            UseSource
        }


        public enum EditorMode
        {
            Rules, 
            Paths,
            Results
        }
    }
}


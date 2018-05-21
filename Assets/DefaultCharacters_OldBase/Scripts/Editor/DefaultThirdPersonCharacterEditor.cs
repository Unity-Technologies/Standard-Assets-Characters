using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(DefaultThirdPersonCharacter))]
public class DefaultThirdPersonCharacterEditor : Editor
{
    SerializedProperty m_ScriptProp;
    SerializedProperty m_CameraTransformProp;
    SerializedProperty m_MaxForwardSpeedProp;
    SerializedProperty m_UseAccelProp;
    SerializedProperty m_GroundAccelProp;
    SerializedProperty m_GroundDecelProp;
    SerializedProperty m_AirAccelProp;
    SerializedProperty m_AirDecelProp;
    SerializedProperty m_GravityProp;
    SerializedProperty m_JumpSpeedProp;
    SerializedProperty m_InterpolTurningProp;
    SerializedProperty m_TurnSpeedProp;
    SerializedProperty m_AirTurnSpeedProp;


    private readonly static int k_HashLocomotionState = Animator.StringToHash ("Locomotion");


    private void OnEnable ()
    {
        m_ScriptProp = serializedObject.FindProperty ("m_Script");
        m_CameraTransformProp = serializedObject.FindProperty ("cameraTransform");
        m_MaxForwardSpeedProp = serializedObject.FindProperty ("maxForwardSpeed");
        m_UseAccelProp = serializedObject.FindProperty ("useAcceleration");
        m_GroundAccelProp = serializedObject.FindProperty ("groundAcceleration");
        m_GroundDecelProp = serializedObject.FindProperty ("groundDeceleration");
        m_AirAccelProp = serializedObject.FindProperty ("airborneAccelProportion");
        m_AirDecelProp = serializedObject.FindProperty ("airborneDecelProportion");
        m_GravityProp = serializedObject.FindProperty ("gravity");
        m_JumpSpeedProp = serializedObject.FindProperty ("jumpSpeed");
        m_InterpolTurningProp = serializedObject.FindProperty ("interpolateTurning");
        m_TurnSpeedProp = serializedObject.FindProperty ("turnSpeed");
        m_AirTurnSpeedProp = serializedObject.FindProperty ("airborneTurnSpeedProportion");
    }


    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();

        GUI.enabled = false;
        EditorGUILayout.PropertyField (m_ScriptProp);
        GUI.enabled = true;
        
        EditorGUILayout.PropertyField (m_CameraTransformProp);

        EditorGUILayout.PropertyField (m_MaxForwardSpeedProp);

        EditorGUILayout.PropertyField (m_UseAccelProp);

        if (m_UseAccelProp.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(m_GroundAccelProp);
            EditorGUILayout.PropertyField(m_GroundDecelProp);
            EditorGUILayout.PropertyField(m_AirAccelProp);
            EditorGUILayout.PropertyField(m_AirDecelProp);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(m_GravityProp);
        EditorGUILayout.PropertyField(m_JumpSpeedProp);

        EditorGUILayout.PropertyField(m_InterpolTurningProp);

        if (m_InterpolTurningProp.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField (m_TurnSpeedProp);
            EditorGUILayout.PropertyField (m_AirTurnSpeedProp);

            EditorGUI.indentLevel--;
        }

        MonitorLocomotionAnimationSpeed(PlayModeStateChange.EnteredEditMode);
        
        serializedObject.ApplyModifiedProperties ();
    }


    [InitializeOnLoadMethod]
    private static void Initialize ()
    {
        EditorApplication.playModeStateChanged += MonitorLocomotionAnimationSpeed;
    }


    private static void MonitorLocomotionAnimationSpeed (PlayModeStateChange stateChange)
    {
        DefaultThirdPersonCharacter[] allDefaultTPCs = FindObjectsOfType<DefaultThirdPersonCharacter> ();

        for (int i = 0; i < allDefaultTPCs.Length; i++)
        {
            Animator animator = allDefaultTPCs[i].GetComponent<Animator> ();

            if(animator == null)
                throw new UnityException("There is no animator connected to the DefaultThirdPersonCharacter - " + allDefaultTPCs[i].name + ".");

            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;

            if (controller == null)
                throw new UnityException("There is no controller assigned to the animator of DefaultThirdPersonCharacter - " + allDefaultTPCs[i].name + ".");

            AnimatorControllerLayer baseLayer = controller.layers[0];

            if (baseLayer == null)
                throw new UnityException("There is no base layer in the AnimatorController of DefaultThirdPersonCharacter - " + allDefaultTPCs[i].name + ".");

            ChildAnimatorState[] baseLayerStates = baseLayer.stateMachine.states;

            AnimatorState locomotionState = null;

            for (int j = 0; j < baseLayerStates.Length; j++)
            {
                if (baseLayerStates[j].state.nameHash == k_HashLocomotionState)
                    locomotionState = baseLayerStates[j].state;
            }

            if (locomotionState == null)
                throw new UnityException("There is no state called 'Locomotion' on the base layer of the AnimatorController of DefaultThirdPersonCharacter - " + allDefaultTPCs[i].name + ".");

            BlendTree locomotionBlendTree = locomotionState.motion as BlendTree;

            if (locomotionBlendTree == null)
                throw new UnityException("There is no BlendTree in the Locomotion state of the AnimatorController of DefaultThirdPersonCharacter - " + allDefaultTPCs[i].name + ".");

            int topSpeedIndex = 0;

            ChildMotion[] childMotions = locomotionBlendTree.children;

            for (int j = 0; j < childMotions.Length; j++)
            {
                ChildMotion childMotion = childMotions[j];

                if (childMotion.threshold >= childMotions[topSpeedIndex].threshold)
                {
                    topSpeedIndex = j;
                }
            }

            if (allDefaultTPCs[i].maxForwardSpeed > childMotions[topSpeedIndex].threshold)
            {
                childMotions[topSpeedIndex].timeScale = allDefaultTPCs[i].maxForwardSpeed / childMotions[topSpeedIndex].threshold;
                childMotions[topSpeedIndex].threshold = allDefaultTPCs[i].maxForwardSpeed;
                locomotionBlendTree.children = childMotions;
            }
        }
    }
}

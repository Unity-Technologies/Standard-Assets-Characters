using System;
using System.Collections.Generic;
using Cinemachine.Editor;
using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// COMMENT TODO
    /// </summary>
    [CustomPropertyDrawer(typeof(MovementEventZoneDefinitionList))]
    public class MovementEventZoneDefinitionListPropertyDrawer : PropertyDrawer
    {
        // COMMENT TODO
        const float k_ArrayElementHeightScale = 1.1f;

        // COMMENT TODO
        ReorderableList m_ReorderableList;
        
        // COMMENT TODO
        SerializedProperty rootProperty;
        
        // COMMENT TODO
        SerializedProperty listProperty;
        
        // COMMENT TODO
        List<float> m_ElementHeights = new List<float>();


        /// <summary>
        /// COMMENT TODO
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return m_ReorderableList == null ? base.GetPropertyHeight(property, label) : m_ReorderableList.GetHeight();
        }

        /// <summary>
        /// COMMENT TODO
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetupReorderableList(property);
            m_ReorderableList.DoList(position);
            rootProperty.serializedObject.ApplyModifiedProperties();
        }

        // COMMENT TODO
        void SetupReorderableList(SerializedProperty property)
        {
            if (m_ReorderableList != null)
            {
                return;
            }

            rootProperty = property;

            listProperty = property.FindPropertyRelative("m_MovementZoneLibraries");
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                m_ElementHeights.Add(2 * EditorGUIUtility.singleLineHeight);
            }

            m_ReorderableList = new ReorderableList(listProperty.serializedObject, listProperty, false, false, true, true);
            m_ReorderableList.drawHeaderCallback = rect =>
            {
                GUI.Label(rect, "Movement Zones");
            };

            m_ReorderableList.drawElementCallback = DrawElementCallback;
            m_ReorderableList.elementHeightCallback = ElementHeightCallback;
            m_ReorderableList.onAddCallback = OnAddCallback;
            m_ReorderableList.onRemoveCallback = OnRemoveCallback;
            m_ReorderableList.onChangedCallback = OnChangedCallback; 
        }

        // COMMENT TODO
        void OnChangedCallback(ReorderableList list)
        {
            rootProperty.serializedObject.ApplyModifiedProperties();
        }

        // COMMENT TODO
        void OnRemoveCallback(ReorderableList list)
        {
            m_ElementHeights.RemoveAt(list.index);

            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            if (list.index >= list.serializedProperty.arraySize - 1)
            {
                list.index = list.serializedProperty.arraySize - 1;
            }
        }

        // COMMENT TODO
        void OnAddCallback(ReorderableList list)
        {
            m_ElementHeights.Add(2 * EditorGUIUtility.singleLineHeight);
            listProperty.arraySize++;
        }

        // COMMENT TODO
        float ElementHeightCallback(int index)
        {
            return m_ElementHeights[index];
        }

        // COMMENT TODO
        void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var elementHeight = EditorGUIUtility.singleLineHeight;
            var elementProperty = listProperty.GetArrayElementAtIndex(index);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, elementProperty.FindPropertyRelative("m_PhysicMaterial"), true);
            EditorGUI.indentLevel++;
            rect.y += EditorGUIUtility.singleLineHeight;
            elementHeight += EditorGUIUtility.singleLineHeight;
            var zoneLibary = elementProperty.FindPropertyRelative("m_ZoneLibrary");
            EditorGUI.PropertyField(rect, zoneLibary, true);
            EditorGUI.indentLevel--;
            if (zoneLibary.isExpanded)
            {
                elementHeight += EditorGUIUtility.singleLineHeight;
                elementHeight += GetExpandedArrayHeight(zoneLibary.FindPropertyRelative("m_LeftFootStepPrefabs"));
                elementHeight += GetExpandedArrayHeight(zoneLibary.FindPropertyRelative("m_RightFootStepPrefabs"));
                elementHeight += GetExpandedArrayHeight(zoneLibary.FindPropertyRelative("m_LandingPrefabs"));
                elementHeight += GetExpandedArrayHeight(zoneLibary.FindPropertyRelative("m_JumpingPrefabs"));
            }

            elementHeight += EditorGUIUtility.singleLineHeight;
            m_ElementHeights[index] = elementHeight;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(rootProperty.serializedObject.targetObject);
            }
        }

        // COMMENT TODO
        float GetExpandedArrayHeight(SerializedProperty arrayElement)
        {
            if (arrayElement.isExpanded)
            {
                return ((arrayElement.arraySize + 1) * EditorGUIUtility.singleLineHeight * k_ArrayElementHeightScale) + EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }
    }
}

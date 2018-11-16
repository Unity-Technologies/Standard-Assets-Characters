using System;
using System.Collections.Generic;
using Cinemachine.Editor;
using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace Editor
{
    [CustomPropertyDrawer(typeof(MovementEventZoneDefinitionList))]
    public class MovementEventZoneDefinitionListPropertyDrawer : PropertyDrawer
    {
        const float k_ArrayElementHeightScale = 1.1f;
        
        ReorderableList m_ReorderableList;
        SerializedProperty listProperty;
        List<float> m_ElementHeights = new List<float>();

        void SetupReorderableList(SerializedProperty property)
        {
            if (m_ReorderableList != null)
            {
                return;
            }
            
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
//            m_ReorderableList.onAddCallback = OnAddCallback;
//            m_ReorderableList.onRemoveCallback = OnRemoveCallback;
        }

//        void OnRemoveCallback(ReorderableList list)
//        {
//            m_ElementHeights.RemoveAt(list.index);
//            list.list.RemoveAt(list.index);
//            //EditorUtility.SetDirty(listProperty.objectReferenceValue);
//        }
//
//        void OnAddCallback(ReorderableList list)
//        {
//            m_ElementHeights.Add(2 * EditorGUIUtility.singleLineHeight);
//            //listProperty.Add(new MovementEventZoneDefinition());
//            EditorUtility.SetDirty(listProperty.objectReferenceValue);
//        }

        float ElementHeightCallback(int index)
        {
            return m_ElementHeights[index];
        }

        void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var elementHeight = EditorGUIUtility.singleLineHeight;
            var elementProperty = listProperty.GetArrayElementAtIndex(index);
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
        }

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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        { 
            SetupReorderableList(property);   
            m_ReorderableList.DoLayoutList();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}

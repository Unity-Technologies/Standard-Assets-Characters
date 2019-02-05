using System;
using StandardAssets.Characters.Physics;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.Editor
{
	/// <summary>
	/// Custom editor for rendering advance fields in the <see cref="OpenCharacterController"/>
	/// </summary>
	[CustomEditor(typeof(OpenCharacterController))]
	public class OpenCharacterControllerEditor : UnityEditor.Editor
	{
		const string k_MinSlowAgainstWallsAngle = "m_MinSlowAgainstWallsAngle";

		// List of names of advanced fields
		readonly string[] m_AdvancedFields =
		{
			"m_SkinWidth",
			"m_MinMoveDistance",
			"m_IsLocalHuman",
			"m_SlideAlongCeiling",
			"m_TriggerQuery"
		};

		// Tracks the whether the advanced foldout is open/collapse
		bool m_AdvancedFoldOut;


		/// <summary>
		/// Renders advanced fields
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, GetExclusions());

			EditorGUILayout.Space();
			serializedObject.DrawFieldsUnderFoldout("Advanced", m_AdvancedFields, ref m_AdvancedFoldOut);

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		string[] GetExclusions()
		{
			var controller = (OpenCharacterController) target;

			if (!controller.slowAgainstWalls)
			{
				var array = new string[m_AdvancedFields.Length + 1];

				Array.Copy(m_AdvancedFields, array, m_AdvancedFields.Length);
				array[m_AdvancedFields.Length] = k_MinSlowAgainstWallsAngle;

				return array;
			}

			return m_AdvancedFields;
		}
	}
}

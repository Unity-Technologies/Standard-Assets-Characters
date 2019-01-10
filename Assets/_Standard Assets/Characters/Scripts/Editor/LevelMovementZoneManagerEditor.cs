using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.Editor
{
	/// <summary>
	/// Custom  editor for <see cref="LevelMovementZoneManager"/>
	/// </summary>
	[CustomEditor(typeof(LevelMovementZoneManager))]
	public class LevelMovementZoneManagerEditor : UnityEditor.Editor
	{
		// Configuration field name
		const string k_Configuration = "m_Configuration";

		// Field drawing exclusion
		string[] m_Exclusions = { k_Configuration };
		

		/// <summary>
		/// Handles drawing the inline scriptable object editor
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, m_Exclusions);
			
			serializedObject.DrawExtendedScriptableObject(k_Configuration);
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
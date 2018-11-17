using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// COMMENT TODO
	/// </summary>
	[CustomEditor(typeof(LevelMovementZoneManager))]
	public class LevelMovementZoneManagerEditor : UnityEditor.Editor
	{
		// COMMENT TODO
		const string k_Configuration = "m_Configuration";

		// COMMENT TODO
		string[] exclusions = { k_Configuration };
		

		/// <summary>
		/// COMMENT TODO
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, exclusions);
			
			serializedObject.DrawExtendedScriptableObject(k_Configuration);
			
			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
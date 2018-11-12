using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(LevelMovementZoneManager))]
	public class LevelMovementZoneManagerEditor : UnityEditor.Editor
	{
		const string k_Configuration = "m_Configuration";

		string[] exclusions = { k_Configuration };
		
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
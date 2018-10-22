using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(LevelMovementZoneManager))]
	public class LevelMovementZoneManagerEditor : UnityEditor.Editor
	{
		private const string k_Configuration = "configuration";
		
		private string[] exclusions = new string[]
		{
			k_Configuration
		};
		
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
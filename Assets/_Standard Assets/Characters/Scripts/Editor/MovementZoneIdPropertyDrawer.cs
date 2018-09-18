using System.Collections.Generic;
using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEngine;

namespace StandardAssets._Standard_Assets.Characters.Scripts.Editor
{
	[CustomPropertyDrawer(typeof(MovementZoneIdAttribute))]
	public class MovementZoneIdPropertyDrawer  : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			string[] guids =
				AssetDatabase.FindAssets(string.Format("t: {0}", typeof(MovementZoneIdDefinition)));

			if (guids.Length == 0)
			{
				base.OnGUI(position, property, label);
				return;
			}

			MovementZoneIdDefinition definition =
				AssetDatabase.LoadAssetAtPath<MovementZoneIdDefinition>(AssetDatabase.GUIDToAssetPath(guids[0]));

			string s = property.stringValue;
			int index = EditorGUI.Popup(position, property.displayName, GetIndex(s, definition.ids), 
			                            definition.ids);
			property.stringValue = definition.ids[index];
		}
		
		private int GetIndex(string selected, IList<string> parameters)
		{
			for (int index = 0; index < parameters.Count; index++)
			{
				string stateName = parameters[index];
				if (stateName == selected)
				{
					return index;
				}
			}
			return 0;
		}
	}
}
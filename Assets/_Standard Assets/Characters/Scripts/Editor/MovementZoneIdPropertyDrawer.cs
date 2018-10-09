using System.Collections.Generic;
using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Effects;
using StandardAssets.Characters.Effects.Configs;
using UnityEditor;
using UnityEngine;

namespace StandardAssets._Standard_Assets.Characters.Scripts.Editor
{
	/// <summary>
	/// Property drawer for <see cref="MovementZoneIdAttribute"/>
	/// </summary>
	[CustomPropertyDrawer(typeof(MovementZoneIdAttribute))]
	public class MovementZoneIdPropertyDrawer  : PropertyDrawer
	{
		/// <summary>
		/// Draws a dropdown list of options specified in the <see cref="ZoneIdsConfig"/>
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			string[] guids =
				AssetDatabase.FindAssets(string.Format("t: {0}", typeof(ZoneIdsConfig)));

			if (guids.Length == 0)
			{
				Debug.LogError("Did not find the ZoneIdsConfig");
				base.OnGUI(position, property, label);
				return;
			}

			if (guids.Length > 1)
			{
				Debug.LogError("Found multiple MovementZoneIdDefinitions - there should be only one");
				base.OnGUI(position, property, label);
				return;
			}

			ZoneIdsConfig definition =
				AssetDatabase.LoadAssetAtPath<ZoneIdsConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));

			string s = property.stringValue;
			int index = EditorGUI.Popup(position, property.displayName, GetIndex(s, definition.ids), 
			                            definition.ids);
			property.stringValue = definition.ids[index];
		}
		
		/// <summary>
		/// Gets the currently selected index
		/// </summary>
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
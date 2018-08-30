using System.Linq;
using StandardAssets.Characters.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomPropertyDrawer(typeof(DisableEditAtRuntimeAttribute))]
	public class DisableEditAtRuntimePropertyDrawer : PropertyDrawer
	{

		private DisableEditAtRuntimeAttribute DisableEditAtRuntimeAttribute
		{
			get { return (DisableEditAtRuntimeAttribute) attribute; }
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var noEntrySign = Resources.Load<Texture2D>("Texture set/Texture/Editor/NoEntry");
			
			if (Application.isPlaying)
			{
				GUIContent noEntryLabel = new GUIContent(property.displayName, noEntrySign);
				
				if (noEntrySign != null)
				{
					GUI.enabled = false;
					EditorGUI.PropertyField(position, property, noEntryLabel, true);
					GUI.enabled = true;
				}
				else
				{
					GUI.enabled = false;
					EditorGUI.PropertyField(position, property, label, true);
					GUI.enabled = true;
				}
			}
			else
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
		}
		
	}
}
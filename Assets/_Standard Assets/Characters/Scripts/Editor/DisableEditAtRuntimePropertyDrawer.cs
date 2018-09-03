using System.Linq;
using StandardAssets.Characters.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// Custom property drawer for the <see cref="DisableEditAtRuntimeAttribute"/>
	/// This will disable the editing of a field when the Unity Editor is in Play Mode.
	/// </summary>
	[CustomPropertyDrawer(typeof(DisableEditAtRuntimeAttribute))]
	public class DisableEditAtRuntimePropertyDrawer : PropertyDrawer
	{

		private DisableEditAtRuntimeAttribute DisableEditAtRuntimeAttribute
		{
			get { return (DisableEditAtRuntimeAttribute) attribute; }
		}

		///<inheritdoc />
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}

		///<inheritdoc />
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
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

		/// <summary>
		/// Gets the height of the relevant property in order to correctly format the inspector when this property is drawn 
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label);
		}

		/// <summary>
		/// Will draw the relevant property on the inspector in read-only mode when the inspector is in play-mode and the OnGui method is triggered  
		/// </summary>
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
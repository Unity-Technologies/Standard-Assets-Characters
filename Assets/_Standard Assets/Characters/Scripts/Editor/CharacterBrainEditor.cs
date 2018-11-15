using StandardAssets.Characters.Common;
using StandardAssets.Characters.FirstPerson;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(CharacterBrain))]
	public abstract class CharacterBrainEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, GetExclusions());

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		protected abstract string[] GetExclusions();
	}

	[CustomEditor(typeof(FirstPersonBrain))]
	public class FirstPersonBrainEditor : CharacterBrainEditor
	{
		protected override string[] GetExclusions()
		{
			return new string[0];
		}
	}
}
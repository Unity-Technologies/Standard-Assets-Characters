using StandardAssets.Characters.Common;
using StandardAssets.Characters.FirstPerson;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// COMMENT TODO
	/// </summary>
	[CustomEditor(typeof(CharacterBrain))]
	public abstract class CharacterBrainEditor : UnityEditor.Editor
	{
		/// <summary>
		/// COMMENT TODO
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawPropertiesExcluding(serializedObject, GetExclusions());

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		/// <summary>
		/// COMMENT TODO
		/// </summary>
		protected abstract string[] GetExclusions();
	}


	/// <summary>
	/// COMMENT TODO
	/// </summary>
	[CustomEditor(typeof(FirstPersonBrain))]
	public class FirstPersonBrainEditor : CharacterBrainEditor
	{
		/// <summary>
		/// COMMENT TODO
		/// </summary>
		protected override string[] GetExclusions()
		{
			return new string[0];
		}
	}
}
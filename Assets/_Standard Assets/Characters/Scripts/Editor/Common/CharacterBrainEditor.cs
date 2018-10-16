using StandardAssets.Characters.Common;
using StandardAssets.Characters.Physics;
using UnityEditor;
using UnityEngine;

namespace Editor.Common
{
	[CustomEditor(typeof(CharacterBrain))]
	public abstract class CharacterBrainEditor : UnityEditor.Editor
	{
		protected const string k_CharacterControllerAdapterName = "characterControllerAdapter";
		protected const string k_OpenCharacterControllerAdapterName = "openCharacterControllerAdapter";

		protected string[] characterControllerExclusions, openCharacterControllerExclusions;
		
		protected abstract string[] baseExclusions { get; }
		
		public override void OnInspectorGUI()
		{
			CharacterBrain brain = (CharacterBrain)target;
			
			CreateExclusionList(ref characterControllerExclusions, k_OpenCharacterControllerAdapterName);
			CreateExclusionList(ref openCharacterControllerExclusions, k_CharacterControllerAdapterName);
			
			if (brain.GetComponent<OpenCharacterController>() != null)
			{
				DrawPropertiesExcluding(serializedObject, openCharacterControllerExclusions);
			}
			else if (brain.GetComponent<CharacterController>() != null)
			{
				DrawPropertiesExcluding(serializedObject, characterControllerExclusions);
			}
			else
			{
				Debug.LogError("Could not find OpenCharacterController or CharacterController");
			}
		}

		protected void CreateExclusionList(ref string[] exclusionList, string exclusion)
		{
			if (exclusionList != null && exclusionList.Length > 0)
			{
				return;
			}
			
			exclusionList = CalculateExcludedProperties(exclusion);
		}

		protected string[] CalculateExcludedProperties(string exclusion)
		{
			string[] exclusions = new string[baseExclusions.Length + 1];
			exclusions[0] = exclusion;
			for (int i = 0; i < baseExclusions.Length; i++)
			{
				exclusions[i + 1] = baseExclusions[i];
			}

			return exclusions;
		}
		
		
	}
}
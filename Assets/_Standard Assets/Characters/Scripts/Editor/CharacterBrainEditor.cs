using System.Collections.Generic;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Examples.SimpleMovementController;
using StandardAssets.Characters.FirstPerson;
using StandardAssets.Characters.Physics;
using StandardAssets.Characters.ThirdPerson;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(CharacterBrain))]
	public abstract class CharacterBrainEditor : UnityEditor.Editor
	{
		protected const string k_CharacterControllerAdapterName = "characterControllerAdapter";
		protected const string k_OpenCharacterControllerAdapterName = "openCharacterControllerAdapter";

		public override void OnInspectorGUI()
		{
			CharacterBrain brain = (CharacterBrain) target;

			if (brain.GetComponent<OpenCharacterController>() != null)
			{
				DrawPropertiesExcluding(serializedObject, GetOpenCharacterControllerExclusions());
			}
			else if (brain.GetComponent<CharacterController>() != null)
			{
				DrawPropertiesExcluding(serializedObject, GetCharacterControllerExclusions());
			}
			else
			{
				Debug.LogError("Could not find OpenCharacterController or CharacterController");
			}
		}

		protected virtual string[] GetOpenCharacterControllerExclusions()
		{
			return new string[] {k_CharacterControllerAdapterName};
		}

		protected virtual string[] GetCharacterControllerExclusions()
		{
			return new string[] {k_OpenCharacterControllerAdapterName};
		}
	}

	[CustomEditor(typeof(CapsuleBrain))]
	public class CapsuleBrainEditor : CharacterBrainEditor
	{
	}

	[CustomEditor(typeof(FirstPersonBrain))]
	public class FirstPersonBrainEditor : CharacterBrainEditor
	{
	}

	[CustomEditor(typeof(ThirdPersonBrain))]
	public class ThirdPersonBrainEditor : CharacterBrainEditor
	{
		protected const string k_AnimationTurnaroundName = "animationTurnaroundBehaviour",
		                       k_BlendspaceTurnaroundName = "blendspaceTurnaroundBehaviour";

		protected override string[] GetOpenCharacterControllerExclusions()
		{
			List<string> exclusionList = new List<string> {k_CharacterControllerAdapterName};
			exclusionList.AddRange(GetTurnaroundExclusions());
			return exclusionList.ToArray();
		}

		protected override string[] GetCharacterControllerExclusions()
		{
			List<string> exclusionList = new List<string> {k_OpenCharacterControllerAdapterName};
			exclusionList.AddRange(GetTurnaroundExclusions());
			return exclusionList.ToArray();
		}

		private List<string> GetTurnaroundExclusions()
		{
			ThirdPersonBrain brain = target as ThirdPersonBrain;

			switch (brain.typeOfTurnaround)
			{
				case TurnaroundType.Animation:
					return new List<string>() {k_BlendspaceTurnaroundName};
				case TurnaroundType.Blendspace:
					return new List<string>() {k_AnimationTurnaroundName};
				default:
					return new List<string>() {k_AnimationTurnaroundName, k_BlendspaceTurnaroundName};
			}
		}
	}
}
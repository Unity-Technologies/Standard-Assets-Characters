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
			return new string[]
				{k_CharacterControllerAdapterName, k_AnimationTurnaroundName, k_BlendspaceTurnaroundName};
		}

		protected override string[] GetCharacterControllerExclusions()
		{
			return new string[]
				{k_OpenCharacterControllerAdapterName, k_AnimationTurnaroundName, k_BlendspaceTurnaroundName};
		}
	}
}
using Editor.Common;
using StandardAssets.Characters.ThirdPerson;
using UnityEditor;

namespace Editor.ThirdPerson
{
	[CustomEditor(typeof(ThirdPersonBrain))]
	public class ThirdPersonBrainEditor : CharacterBrainEditor
	{
		protected override string[] baseExclusions
		{
			get { return new string[0];}
		}
	}
}
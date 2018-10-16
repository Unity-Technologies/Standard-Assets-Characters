using Editor.Common;
using StandardAssets.Characters.FirstPerson;
using UnityEditor;

namespace Editor.FirstPerson
{
	[CustomEditor(typeof(FirstPersonBrain))]
	public class FirstPersonBrainEditor : CharacterBrainEditor
	{
		protected override string[] baseExclusions
		{
			get { return new string[0];}
		}
	}
}
using Editor.Common;
using StandardAssets.Characters.Examples.SimpleMovementController;
using UnityEditor;

namespace Editor.SimpleMovementController
{
	[CustomEditor(typeof(CapsuleBrain))]
	public class CapsuleBrainEditor : CharacterBrainEditor
	{
		protected override string[] baseExclusions
		{
			get { return new string[0];}
		}
	}
}
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	public class CapsuleInput : CharacterInput
	{
		protected override Vector2 ConditionMoveInput(Vector2 rawMoveInput)
		{			
			return rawMoveInput;
		}

		protected override void RegisterAdditionalInputs()
		{
			Debug.Log("No extra inputs");
		}

		protected override void RegisterAdditionalInputsMobile()
		{
		}
	}
}
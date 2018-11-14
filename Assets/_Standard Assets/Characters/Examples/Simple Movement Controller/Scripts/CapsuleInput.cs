using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	public class CapsuleInput : CharacterInput
	{
		protected override void RegisterAdditionalInputs()
		{
			Debug.Log("No extra inputs");
		}

		protected override void RegisterAdditionalTouchInputs()
		{
		}
	}
}
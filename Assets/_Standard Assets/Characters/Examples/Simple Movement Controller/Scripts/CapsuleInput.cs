using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovementController
{
	/// <summary>
	/// Implementation of <see cref="CharacterInput"/> with no additional inputs 
	/// </summary>
	public class CapsuleInput : CharacterInput
	{
		/// <summary>
		/// No extra inputs to be registered
		/// </summary>
		protected override void RegisterAdditionalInputs()
		{
		}

		protected override void DeRegisterAdditionalInputs()
		{
		}
	}
}
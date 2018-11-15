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
			Debug.Log("No extra inputs");
		}

		/// <summary>
		/// No extra inputs to be registered
		/// </summary>
		protected override void RegisterAdditionalTouchInputs()
		{
			Debug.Log("No extra inputs");
		}
	}
}
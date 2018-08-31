using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Input responses for onscreen buttons
	/// </summary>
	/// <seealso cref="InputResponse"/>
	[CreateAssetMenu(fileName = "OnscreenInputResponse", menuName = "Standard Assets/Characters/Input/Create Onscreen Response",
		order = 1)]
	public class LegacyOnScreenInputResponse : InputResponse
	{
		/// <inheritdoc />
		/// <summary>
		/// No initialization needed so just logs the name of the control
		/// </summary>
		public override void Init()
		{
			Debug.LogFormat("Initialized onscreen input response = {0}", name);
		}
		
		/// <summary>
		/// Toggles the touch button
		/// </summary>
		/// <param name="touchToggle"></param>
		public void TouchToggle(bool touchToggle)
		{
			if (touchToggle)
			{
				OnInputStarted();
				touchToggle = true;
			}
			else
			{
				OnInputEnded();
				touchToggle = false;
			}
		}
	}
}
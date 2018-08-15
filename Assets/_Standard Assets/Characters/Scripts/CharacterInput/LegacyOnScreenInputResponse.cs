using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	[CreateAssetMenu(fileName = "OnscreenInputResponse", menuName = "Input Response/Create Onscreen Response",
		order = 1)]
	public class LegacyOnScreenInputResponse : InputResponse
	{
		public override void Init()
		{
			
		}
		
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
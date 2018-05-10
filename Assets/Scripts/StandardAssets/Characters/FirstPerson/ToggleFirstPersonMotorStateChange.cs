using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.FirstPerson
{
	public class ToggleFirstPersonMotorStateChange : FirstPersonMotorStateChange
	{
		public KeyCode key;
		
		bool isOn = false;

		void Update()
		{
			if (UnityInput.GetKeyDown(key))
			{
				if (isOn)
				{
					ResetState();
				}
				else
				{
					ChangeState();
				}

				isOn = !isOn;
			}
		}
	}
}
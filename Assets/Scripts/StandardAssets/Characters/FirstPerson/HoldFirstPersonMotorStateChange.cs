using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.FirstPerson
{
	public class HoldFirstPersonMotorStateChange : FirstPersonMotorStateChange
	{
		public KeyCode key;
		
		bool prevInputApplied = false;

		void Update()
		{
			bool keyPressed = UnityInput.GetKey(key);
			
			if (!prevInputApplied && keyPressed)
			{
				ChangeState();
			}

			if (prevInputApplied && !keyPressed)
			{
				ResetState();
			}

			prevInputApplied = keyPressed;
		}
	}
}
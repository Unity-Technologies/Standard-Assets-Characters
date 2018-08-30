using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	public class LegacyOnScreenCharacterInput : LegacyCharacterInputBase
	{
		[SerializeField]
		protected StaticOnScreenJoystick leftOnScreenJoystick;
		[SerializeField]
		protected StaticOnScreenJoystick rightOnScreenJoystick;
		
		protected override void UpdateLookVector()
		{
			Vector2 rightStickVector = rightOnScreenJoystick.GetStickVector();
			lookInputVector.x = -rightStickVector.x;
			lookInputVector.y = -rightStickVector.y;
		}

		protected override void UpdateMoveVector()
		{
			Vector2 leftStickVector = leftOnScreenJoystick.GetStickVector();		
			moveInputVector.Set(leftStickVector.x, leftStickVector.y);
		}
		
		public void OnScreenTouchJump()
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}
		}
	}
}
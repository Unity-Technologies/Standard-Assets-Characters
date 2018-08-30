using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Mobile onscreen "gamepad" implementation
	/// </summary>
	public class LegacyOnScreenCharacterInput : LegacyCharacterInputBase
	{
		[SerializeField, Tooltip("Reference to the onscreen joystick for movement")]
		protected StaticOnScreenJoystick moveInputJoystick;
		[SerializeField, Tooltip("Reference to the onscreen joystick for looking")]
		protected StaticOnScreenJoystick lookInputJoystick;
		
		/// <inheritdoc />
		protected override void UpdateLookVector()
		{
			Vector2 rightStickVector = lookInputJoystick.GetStickVector();
			lookInputVector.x = -rightStickVector.x;
			lookInputVector.y = -rightStickVector.y;
		}

		/// <inheritdoc />
		protected override void UpdateMoveVector()
		{
			Vector2 leftStickVector = moveInputJoystick.GetStickVector();		
			moveInputVector.Set(leftStickVector.x, leftStickVector.y);
		}
		
		/// <summary>
		/// Used by jump Unity UI button 
		/// </summary>
		public void OnScreenTouchJump()
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}
		}
	}
}
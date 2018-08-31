using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Mobile onscreen "gamepad" implementation
	/// </summary>
	/// <seealso cref="LegacyCharacterInputBase"/>
	public class LegacyOnScreenCharacterInput : LegacyCharacterInputBase
	{
		[SerializeField, Tooltip("Reference to the onscreen joystick for movement")]
		protected OnScreenJoystick moveInputJoystick;
		[SerializeField, Tooltip("Reference to the onscreen joystick for looking")]
		protected OnScreenJoystick lookInputJoystick;
		
		/// <inheritdoc />
		/// <summary>
		/// Sets look input vector to values from the lookInputJoystick <see cref="OnScreenJoystick"/>
		/// </summary>
		protected override void UpdateLookVector()
		{
			Vector2 lookStickVector = lookInputJoystick.GetStickVector();
			lookInputVector.x = -lookStickVector.x;
			lookInputVector.y = -lookStickVector.y;
		}

		/// <inheritdoc />
		/// <summary>
		/// Sets move input vector to values from the moveInputJoystick <see cref="OnScreenJoystick"/>
		/// </summary>
		protected override void UpdateMoveVector()
		{
			Vector2 moveStickVector = moveInputJoystick.GetStickVector();		
			moveInputVector.Set(moveStickVector.x, moveStickVector.y);
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
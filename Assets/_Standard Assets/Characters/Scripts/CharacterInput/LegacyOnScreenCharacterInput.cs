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

		/// <summary>
		/// Child controls to enable/disable when this component is enabled/disabled.
		/// </summary>
		[Header("Children"), SerializeField, Tooltip("Child controls to enable/disable when this component is enabled/disabled.")]
		protected GameObject childControls;
		
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

		private void Awake()
		{
			// Call this here, because OnEnable/OnDisable is not fired when the game object starts disabled and this component is disabled.
			EnableChildControls(enabled);
		}

		/// <summary>
		/// Enable the child controls.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			EnableChildControls(true);
		}

		/// <summary>
		/// Disable the child controls.
		/// </summary>
		private void OnDisable()
		{
			EnableChildControls(false);
		}

		/// <summary>
		/// Enable/disable the child controls.
		/// </summary>
		private void EnableChildControls(bool enable)
		{
			if (childControls != null)
			{
				childControls.SetActive(enable);
			}
		}
		
		/// <summary>
		/// Called when the jump button is pressed on the UI. It fires the jumpPressed action.
		/// </summary>
		public void OnScreenTouchJump()
		{
			OnJumpPressed();
		}
	}
}
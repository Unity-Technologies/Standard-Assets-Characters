using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Unity original input implementation
	/// </summary>
	public class LegacyCharacterInput : LegacyCharacterInputBase
	{
		[Header("Movement Input Axes")]
		[SerializeField]
		protected string horizontalAxisName = "Horizontal";

		[SerializeField]
		protected string verticalAxisName = "Vertical";

		[SerializeField]
		protected bool useLookInput = true;

		[SerializeField]
		protected string lookXAxisName = "LookX";

		[SerializeField]
		protected string lookYAxisName = "LookY";

		[SerializeField]
		protected string keyboardJumpName = "Jump";

		[SerializeField]
		protected bool enableOnScreenJoystickControls;

		public StaticOnScreenJoystick leftOnScreenJoystick;
		public StaticOnScreenJoystick rightOnScreenJoystick;

		public void OnScreenTouchJump()
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}
		}

		void GetOnScreenJoystickVectors()
		{
			Vector2 leftStickVector = leftOnScreenJoystick.GetStickVector();
			moveInputVector.Set(leftStickVector.x, leftStickVector.y);

			Vector2 rightStickVector = rightOnScreenJoystick.GetStickVector();
			look.x = -rightStickVector.x;
			look.y = -rightStickVector.y;
		}

		protected override void Update()
		{
			base.Update();
			UpdateJump();
		}

		/// <summary>
		/// Update the look vector2, this is used in 3rd person
		/// and allows mouse and controller to both work at the same time.
		/// mouse look will take preference.
		/// IF there is no mouse inputs, it will get gamepad axis.  
		/// </summary>
		protected override void UpdateLookVector()
		{
			if (!enableOnScreenJoystickControls)
			{
				string lookX = LegacyCharacterInputDevicesCache.ResolveControl(lookXAxisName);
				string lookY = LegacyCharacterInputDevicesCache.ResolveControl(lookYAxisName);

				if (lookX == lookXAxisName && lookY == lookYAxisName && !useLookInput)
				{
					return;
				}

				look.x = Input.GetAxis(lookX);
				look.y = Input.GetAxis(lookY);
			}
		}

		protected override void UpdateMoveVector()
		{
			//Update Move Vector
			moveInputVector.Set(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));
			
			if (enableOnScreenJoystickControls)
			{
				GetOnScreenJoystickVectors();
			}
		}

		private void UpdateJump()
		{
			if (Input.GetButtonDown(LegacyCharacterInputDevicesCache.ResolveControl(keyboardJumpName)) ||
			    Input.GetButtonDown("Jump"))
			{
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
		}
	}
}
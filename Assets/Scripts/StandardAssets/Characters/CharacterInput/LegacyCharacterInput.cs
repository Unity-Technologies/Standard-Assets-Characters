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
				string lookX = LegacyCharacterInputDevicesCache.ResolveControl(lookXAxisName);
				string lookY = LegacyCharacterInputDevicesCache.ResolveControl(lookYAxisName);

				if (lookX == lookXAxisName && lookY == lookYAxisName && !useLookInput)
				{
					return;
				}

				lookInputVector.x = Input.GetAxis(lookX);
				lookInputVector.y = Input.GetAxis(lookY);
		}

		protected override void UpdateMoveVector()
		{
			//Update Move Vector
			moveInputVector.Set(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));
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
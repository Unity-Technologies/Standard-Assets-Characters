using System;
using Cinemachine;
using StandardAssets.Characters.FirstPerson;
using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Unity original input implementation
	/// </summary>
	public class LegacyCharacterInput : LegacyCharacterInputBase
	{
		[SerializeField]
		protected string lookXAxisName = "LookX";

		[SerializeField]
		protected string lookYAxisName = "LookY";
		
		[SerializeField]
		protected bool useMouseLookOnly = false;
		
		public bool toggleMouseLookOnly
		{
			get { return useMouseLookOnly;}
			set { useMouseLookOnly = value; }
		}

		
		[Header("Movement Input Axes")]
		[SerializeField]
		protected string horizontalAxisName = "Horizontal";

		[SerializeField]
		protected string verticalAxisName = "Vertical";

		[SerializeField]
		protected string keyboardJumpName = "Jump";

		//[SerializeField] 
		//protected FirstPersonMouseLookPOVCamera povMouseLook;

		protected override void Update()
		{
			base.Update();
			UpdateJump();
		}

		/// <summary>
		//If !useMouseLookOnly, and Gamepad is plugged in, the look
		//will be controlled by gamepad. If !useMouseLookOnly and no gamepad 
		//is plugged in, mouse look axis name will still resolove. 
		/// </summary>
		protected override void UpdateLookVector()
		{
			
			if (useMouseLookOnly)
			{
				lookInputVector.x = Input.GetAxis(lookXAxisName);
				lookInputVector.y = Input.GetAxis(lookYAxisName);
				return;
			}
			
			string lookX = LegacyCharacterInputDevicesCache.ResolveControl(lookXAxisName);
			string lookY = LegacyCharacterInputDevicesCache.ResolveControl(lookYAxisName);

			lookInputVector.x = Input.GetAxis(lookX);
			lookInputVector.y = Input.GetAxis(lookY);
			
		}

		protected override void UpdateMoveVector()
		{
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
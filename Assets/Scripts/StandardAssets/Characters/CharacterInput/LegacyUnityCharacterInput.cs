using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Unity original input implementation
	/// </summary>
	public class LegacyUnityCharacterInput : MonoBehaviour, ICharacterInput
	{
		[Header("Cinemachine Axes")]
		[SerializeField]
		protected string cinemachineLookXAxisName = "Horizontal";
		
		[SerializeField]
		protected string cinemachineLookYAxisName = "Vertical";
		
		[Header("Input Axes")]
		[SerializeField]
		protected string horizontalAxisName = "Horizontal";
		
		[SerializeField]
		protected string verticalAxisName = "Vertical";

		[SerializeField]
		protected bool useLookInput = true;
		
		[SerializeField]
		protected string mouseLookXAxisName = "LookX";
		
		[SerializeField]
		protected string mouseLookYAxisName = "LookY";

		[SerializeField]
		protected string lookXAxisName = "LookX";
		
		[SerializeField]
		protected string lookYAxisName = "LookY";

		[SerializeField]
		protected string keyboardJumpName = "Jump";
		
		[SerializeField]
		protected string ps4JumpName = "JumpPS4";
		
		[SerializeField]
		protected string xBoneJumpName = "JumpXBone";
		
		[SerializeField]
		protected string xBox360JumpName = "JumpXBox360";

		[SerializeField]
		protected string gamepadJumpName;

		[SerializeField]
		protected bool hasGamepad;

		private Vector2 moveInputVector;
		private Action jumped;

		private Vector2 look;

		public Vector2 lookInput
		{
			get { return look; }
		}
		
		public Vector2 moveInput
		{
			get { return moveInputVector; }
		}
		
		public bool hasMovementInput 
		{ 
			get { return moveInput != Vector2.zero; }
		}
		
		public Action jumpPressed
		{
			get { return jumped; }
			set { jumped = value; }
		}

		private void Awake()
		{
			SetGamepadJumpAxis();
		}
		
		/// <summary>
		/// Sets the jump axis name for gamepads that are plugged in.
		/// </summary>
		void SetGamepadJumpAxis()
		{
			if (Input.GetJoystickNames().Length > 0)
			{
				foreach (var joystick in Input.GetJoystickNames())
				{
					if (joystick.Length > 1)
					{
						hasGamepad = true;
						if (joystick.ToLower().Contains("xbox"))
						{
							gamepadJumpName = xBoneJumpName;
							break;
						}
						if (joystick.ToLower().Contains("360"))
						{
							gamepadJumpName = xBox360JumpName;
							break;
						}
						gamepadJumpName = ps4JumpName;
					}
				}
			}
		}
		
		private void OnEnable()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		private void Update()
		{
			UpdateLookVector();
			//Cache the inputs
			moveInputVector.Set(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));
			
			if(Input.GetButtonDown(LegacyCharacterInputDevicesCache.ResolveControl(keyboardJumpName))||Input.GetButtonDown("Jump"))
			{
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
			
		}
		
		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string cinemachineAxisName)
		{
			if (!useLookInput)
			{
				return 0;
			}
			
			if (cinemachineAxisName == cinemachineLookXAxisName)
			{
				return look.x;
			}
			if (cinemachineAxisName == cinemachineLookYAxisName)
			{
				return look.y;
			}

			//TODO
			//UpdateLookVector();
			
			/*
			 * if (cinemachineAxisName == cinemachineLookXAxisName)
			{
				return Input.GetAxis(LegacyCharacterInputDevicesCache.ResolveControl(lookXAxisName));
			}
			if (cinemachineAxisName == cinemachineLookYAxisName)
			{
				return Input.GetAxis(LegacyCharacterInputDevicesCache.ResolveControl(lookYAxisName));
			}
			 */

			return 0;
		}
		
		/// <summary>
		/// Update the look vector2, this is used in 3rd person
		/// and allows mouse and controller to both work at the same time.
		/// mouse look will take preference.
		/// IF there is no mouse inputs, it will get gamepad axis.  
		/// </summary>
		void UpdateLookVector()
		{
			
			if (Input.GetAxis(mouseLookXAxisName) != 0)
			{
				look.x = Input.GetAxis(mouseLookXAxisName);
			}
			else
			{
				look.x = Input.GetAxis(LegacyCharacterInputDevicesCache.ResolveControl(lookXAxisName));
			}

			if (Input.GetAxis(mouseLookYAxisName) != 0)
			{
				look.y = Input.GetAxis(mouseLookYAxisName);
			}
			else
			{
				look.y = Input.GetAxis(LegacyCharacterInputDevicesCache.ResolveControl(lookYAxisName));
			}
			
		}
	}
}
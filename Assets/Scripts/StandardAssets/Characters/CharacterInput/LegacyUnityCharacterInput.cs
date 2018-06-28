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
		protected string gamepadJumpName;

		[SerializeField]
		protected bool hasGamepad;

		[SerializeField]
		protected LegacyCharacterInputDevices devices;

		private Vector2 moveInputVector;
		private Action jumped;

		private Vector2 look;

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
			if (Input.GetJoystickNames().Length > 0)
			{
				
				//Debug active controllers
				foreach (var joystick in Input.GetJoystickNames())
				{
					if (joystick.Length > 0)
					{
						hasGamepad = true;
						if (joystick.ToLower().Contains("xbox"))
						{
							gamepadJumpName = xBoneJumpName;
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
			
			//Cache the inputs
			moveInputVector.Set(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));
			if(Input.GetButtonDown(devices.GetAxisName(keyboardJumpName))||Input.GetButtonDown("Jump"))
			{
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
			/*
			 * if (Input.GetButtonDown(keyboardJumpName)||hasGamepad & Input.GetButtonDown(gamepadJumpName))
			{
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
			 */
			
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
				
				return Input.GetAxis(devices.GetAxisName(lookXAxisName));
			}
			if (cinemachineAxisName == cinemachineLookYAxisName)
			{
				
				return Input.GetAxis(devices.GetAxisName(lookYAxisName));
			}

			return 0;
		}
	}
}
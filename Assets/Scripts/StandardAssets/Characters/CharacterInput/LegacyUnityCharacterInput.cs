using System;
using Cinemachine;
using UnityEngine;

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
		protected string jumpButtonName = "Jump";

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

		private void OnEnable()
		{
			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		private void Update()
		{
			//Cache the inputs
			moveInputVector.Set(Input.GetAxis(horizontalAxisName), Input.GetAxis(verticalAxisName));

			if (Input.GetButtonDown(jumpButtonName))
			{
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}
		
			Debug.Log(Input.GetAxisRaw("XBone rightStick Press Mac"));
			
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
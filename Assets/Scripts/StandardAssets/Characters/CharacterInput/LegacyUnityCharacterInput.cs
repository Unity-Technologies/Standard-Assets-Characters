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
		[SerializeField]
		private string horizontalAxisName = "Horizontal";
		
		[SerializeField]
		private string verticalAxisName = "Vertical";
		
		[SerializeField]
		private KeyCode jumpKey = KeyCode.Space;

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
			if (Input.GetKeyDown(jumpKey))
			{
				
				if (jumpPressed != null)
				{
					jumpPressed();
				}
			}

			if (Input.GetButtonDown("Jump"))
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
		private float LookInputOverride(string axis)
		{
			if (axis == "Horizontal")
			{
				if (Input.GetAxis("Mouse X") != 0)
				{
					return Input.GetAxis("Mouse X");
				}
				else if (Input.GetAxis("RightStickHorizontal") != 0)
				{
					return Input.GetAxis("RightStickHorizontal");
				}
			}
			if (axis == "Vertical")
			{
				if (Input.GetAxis("Mouse Y") != 0)
				{
					return Input.GetAxis("Mouse Y");
				}
				else if (Input.GetAxis("RightStickVertical") != 0)
				{
					return Input.GetAxis("RightStickVertical");
				}
			}

			return 0;
		}
	}
}
using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    public class NewCharacterInputWangJangledMouseFPS :MonoBehaviour, ICharacterInput
    {
         //Cinemachine Look Test
		public float XSensitivity = 2f;
		public float YSensitivity = -2f;
		public bool smooth;
		public float smoothTime = 5f;
 


		public CinemachineVirtualCamera[] VCams;


		
		private CinemachinePOV mPOV;
		
		[SerializeField]
		private NewInputActions controls;

		private Vector2 look;

		private Vector2 sLook;

		public Vector2 moveInput { get; private set; }

		public float lookScale = 5f;

		public bool hasMovementInput 
		{ 
			get { return moveInput != Vector2.zero; }
		}

		public Action jumpPressed { get; set; }
	
		 void Start ()
		 {
			// CinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
			mPOV = VCams[0].GetCinemachineComponent<CinemachinePOV>();
			
		}
		
		public void OnEnable()
		{
			controls.Enable();
			controls.gameplay.movement.performed += Move;
			controls.gameplay.look.performed += Look;
			controls.gameplay.jump.performed += Jump;
			controls.gameplay.gamePadLook.performed += GLook;

			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		public void OnDisable()
		{
			controls.Disable();
			controls.gameplay.movement.performed -= Move;
			controls.gameplay.look.performed -= Look;
			controls.gameplay.gamePadLook.performed -= GLook;
		}

		private void Move(InputAction.CallbackContext ctx)
		{
			moveInput = ctx.ReadValue<Vector2>();
		}

		private void Look(InputAction.CallbackContext ctx)
		{
			look = ctx.ReadValue<Vector2>();

			var rot = getScaledMouseRotation();
			
			foreach (var cam in VCams)
			{
				var vCam = cam.GetCinemachineComponent<CinemachinePOV>();
				vCam.m_HorizontalAxis.Value = rot.y;
				vCam.m_VerticalAxis.Value = rot.x;
			}
		}

		Vector3 getScaledMouseRotation()
		{
			Vector3 rot = new Vector3(mPOV.m_VerticalAxis.Value, mPOV.m_HorizontalAxis.Value, 0);
			Vector3 delta = new Vector3(
				CinemachineCore.GetInputAxis("Mouse Y") * YSensitivity,
				CinemachineCore.GetInputAxis("Mouse X") * XSensitivity, 0);
			Vector3 newRot = rot + delta;
			newRot.x = Mathf.Clamp(
				newRot.x, mPOV.m_VerticalAxis.m_MinValue, mPOV.m_VerticalAxis.m_MaxValue);
			delta = newRot - rot;
			if (smooth)
				delta = Cinemachine.Utility.Damper.Damp(delta, smoothTime, Time.deltaTime);
			rot += delta;

			return rot;
		}

		private void GLook(InputAction.CallbackContext ctx)
		{
			sLook = ctx.ReadValue<Vector2>();
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string axis)
		{
			if (axis == "Mouse X")
			{
				return look.x;
			}

			if (axis == "Mouse Y")
			{
				return look.y;
			}

			if (axis == "Stick X")
			{
				return sLook.x;
			}

			if (axis == "Stick Y")
			{
				return sLook.y;
			}
			return 0;
		}

		private void Jump(InputAction.CallbackContext ctx)
		{
			if (jumpPressed != null)
			{
				jumpPressed();
			}	
		}
    }
}
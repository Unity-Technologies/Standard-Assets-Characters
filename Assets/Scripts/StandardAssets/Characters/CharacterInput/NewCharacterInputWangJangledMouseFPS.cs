using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    public class NewCharacterInputWangJangledMouseFPS :MonoBehaviour, ICharacterInput
    {
         //Cinemachine mouse look sensitivity
		public float XSensitivity = 18f;
		public float YSensitivity = -18f;
		public bool smooth = true;
		public float smoothTime = 5f;
 
		private Vector2 responsiveMouseRotation;
		public CinemachineVirtualCamera[] VCams;	//All VCams for the character	
		private CinemachinePOV mPOV; //POV
		
	    
		[SerializeField]
		private NewInputActions controls;

		private Vector2 mouseLook;

		private Vector2 gamePadLook;

		public Vector2 moveInput { get; private set; }

		

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
			controls.gameplay.gamePadLook.performed += GamePadLookVector;

			CinemachineCore.GetInputAxis = LookInputOverride;
		}

		public void OnDisable()
		{
			controls.Disable();
			controls.gameplay.movement.performed -= Move;
			controls.gameplay.look.performed -= Look;
			controls.gameplay.gamePadLook.performed -= GamePadLookVector;
		}

		private void Move(InputAction.CallbackContext ctx)
		{
			moveInput = ctx.ReadValue<Vector2>();
		}

		/// <summary>
		/// This look method is only for use with mouse look
		/// </summary>
		private void Look(InputAction.CallbackContext ctx)
		{
			mouseLook = ctx.ReadValue<Vector2>();
			responsiveMouseRotation = getResponsiveMouseLook();
			
			// Rotate each camera manually
			// FIX 
			foreach (var cam in VCams)
			{
				var vCam = cam.GetCinemachineComponent<CinemachinePOV>();
				vCam.m_HorizontalAxis.Value = responsiveMouseRotation.y;
				vCam.m_VerticalAxis.Value = responsiveMouseRotation.x;
			}
		}
		
	    /// <summary>
	    /// Manually scales the mouse look vector to eliminate the unresponsive
	    /// look when using mouse input
	    /// </summary>
	    /// <returns></returns>
		Vector3 getResponsiveMouseLook()
		{
			//This method needs to happen FAST else there is noticable input lag.
			Vector3 cameraRotation = new Vector3(mPOV.m_VerticalAxis.Value, mPOV.m_HorizontalAxis.Value, 0);
			Vector3 rotationDelta = new Vector3(
				CinemachineCore.GetInputAxis("Mouse Y") * YSensitivity,
				CinemachineCore.GetInputAxis("Mouse X") * XSensitivity, 0);
			Vector3 newCameraRotation = cameraRotation + rotationDelta;
			
			
			rotationDelta = newCameraRotation - cameraRotation;
			
			if (smooth)
				rotationDelta = Cinemachine.Utility.Damper.Damp(rotationDelta, smoothTime, Time.deltaTime);
			
			
			cameraRotation += rotationDelta;
			
			return cameraRotation;
		}

		private void GamePadLookVector(InputAction.CallbackContext ctx)
		{
			gamePadLook = ctx.ReadValue<Vector2>();
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string axis)
		{
			//NOTE: The gamepad vector comes back normalised, and its magnitude is
			// too small to work well with the updated mouse look calculations
			// this is why it is split 
			if (axis == "Mouse X")
			{
				return mouseLook.x;
			}

			if (axis == "Mouse Y")
			{
				return mouseLook.y;
			}

			if (axis == "Stick X")
			{
				return gamePadLook.x;
			}

			if (axis == "Stick Y")
			{
				return gamePadLook.y;
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
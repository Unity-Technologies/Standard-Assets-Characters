using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.CharacterInput
{
    public class NewCharacterInputWangJangledMouseFPS :MonoBehaviour, ICharacterInput
    {
         //Cinemachine mouse look sensitivity
	    public float XSensitivity = 1;
	    public float YSensitivity = -1;
		public float smoothTime = 1.1f;
	    
	    public float mouseSensitivity { get; set; }
 
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
			mPOV = VCams[0].GetCinemachineComponent<CinemachinePOV>();	
			CinemachineCore.GetInputAxis = LookInputOverride;
		 }
		
		public void OnEnable()
		{
			controls.Enable();
			controls.gameplay.movement.performed += Move;
			controls.gameplay.look.performed += MouseLook;
			controls.gameplay.jump.performed += Jump;
			controls.gameplay.gamePadLook.performed += GamepadLook;
		}

		public void OnDisable()
		{
			controls.Disable();
			controls.gameplay.movement.performed -= Move;
			controls.gameplay.look.performed -= MouseLook;
			controls.gameplay.gamePadLook.performed -= GamepadLook;
		}

		private void Move(InputAction.CallbackContext ctx)
		{
			moveInput = ctx.ReadValue<Vector2>();
		}

		/// <summary>
		/// This look method is only for use with mouse look
		/// </summary>
		private void MouseLook(InputAction.CallbackContext ctx)
		{
			mouseLook = ctx.ReadValue<Vector2>();
			ResponsiveMouseLook();
			
		}
	    
	    private void GamepadLook(InputAction.CallbackContext ctx)
	    {
		    gamePadLook = ctx.ReadValue<Vector2>();
	    }
	    
	    private void Jump(InputAction.CallbackContext ctx)
	    {
		    if (jumpPressed != null)
		    {
			    jumpPressed();
		    }	
	    }
		
	    /// <summary>
	    /// Manually scales the mouse look vector to eliminate acceleration and deceleration times
	    /// </summary>
		void ResponsiveMouseLook()
		{
			var cameraRotation = new Vector3(mPOV.m_VerticalAxis.Value, mPOV.m_HorizontalAxis.Value, 0);
			var rotationDelta = new Vector3(mouseLook.y * YSensitivity, mouseLook.x* XSensitivity, 0);
			
			rotationDelta = Cinemachine.Utility.Damper.Damp(
				(rotationDelta), smoothTime, Time.deltaTime);
			
			cameraRotation += rotationDelta;
			
			//Update all the POV cams on controller 
			foreach (var cam in VCams)
			{
				var vCam = cam.GetCinemachineComponent<CinemachinePOV>();
				vCam.m_HorizontalAxis.Value = cameraRotation.y;
				vCam.m_VerticalAxis.Value = cameraRotation.x;
			}
			
		}

		/// <summary>
		/// Sets the Cinemachine cam POV to mouse inputs.
		/// </summary>
		private float LookInputOverride(string axis)
		{
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

		
    }
}
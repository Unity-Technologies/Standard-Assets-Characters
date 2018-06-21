using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.FirstPerson
{
    public class FirstPersonMouseLookPOVCamera: MonoBehaviour
    {
        /// <summary>
        /// These variables control the speed/sensitivity of the mouse look
        /// They are adjustable in play mode using the UI
        /// If camera starts to jitter, make sure inspector values match these defaults. 
        /// </summary>
        public float XSensitivity = 1; 
        public float YSensitivity = -1;
        public float smoothTime = 1.1f;
        
        /// <summary>
        /// Reference to all the cinemachine vcams on the character
        /// All the VCams must have the 'aim' set to POV 
        /// </summary>
        public CinemachineVirtualCamera[] VCams;	
        private CinemachinePOV mPOV; 

       
        [SerializeField]
        private InputActionReference[] lookActionReferences;
        
        private Vector2 look;
        
        void Start ()
        {
            mPOV = VCams[0].GetCinemachineComponent<CinemachinePOV>();
        }

        void OnEnable()
        {
            foreach (InputActionReference inputActionReference in lookActionReferences)
            {
                inputActionReference.action.performed += Look;
            }
        }

        private void OnDisable()
        {
            foreach (InputActionReference inputActionReference in lookActionReferences)
            {
                inputActionReference.action.performed += Look;
            }
        }

        
        void Look(InputAction.CallbackContext ctx)
        {
            look = ctx.ReadValue<Vector2>();
            
        }

        void Update()
        {
            ResponsiveMouseLook();
        }
        void ResponsiveMouseLook()
        {
            var cameraRotation = GetCameraRotationVector();	
            UpdatePovCameras(cameraRotation);
        }
        
        /// <summary>
        /// Update each cameras Horizontal and Vertical axis values
        /// </summary>
        /// <param name="cameraRotation"></param>
        void UpdatePovCameras(Vector3 cameraRotation)
        {
            foreach (var cam in VCams)
            {
                var vCam = cam.GetCinemachineComponent<CinemachinePOV>();
                vCam.m_HorizontalAxis.Value = cameraRotation.y;
                vCam.m_VerticalAxis.Value = cameraRotation.x;
            }
        }
        
        /// <summary>
        /// Manually scales the mouse look vector to eliminate acceleration and deceleration times felt
        /// when using the mouse to control the POV cinemachine cameras
        /// </summary>
        Vector3 GetCameraRotationVector()
        {
            var cameraRotation = new Vector3(mPOV.m_VerticalAxis.Value, mPOV.m_HorizontalAxis.Value, 0);
            var rotationDelta = new Vector3(look.y * YSensitivity, look.x* XSensitivity, 0);
			
            rotationDelta = Cinemachine.Utility.Damper.Damp(rotationDelta, smoothTime, Time.deltaTime);
			
            cameraRotation += rotationDelta;

            return cameraRotation;
        }

    }
}
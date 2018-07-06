using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.FirstPerson
{
    public class FirstPersonMouseLookPOVCamera: MonoBehaviour
    {
        [SerializeField]
        protected string lookXAxisName = "LookX";
		
        [SerializeField]
        protected string lookYAxisName = "LookY";
        
        /// <summary>
        /// These variables control the speed/sensitivity of the mouse look
        /// They are adjustable in play mode using the UI
        /// If camera starts to jitter, make sure inspector values match these defaults. 
        /// </summary>
        [SerializeField]
        protected float XSensitivity = 1; 
        [SerializeField]
        protected float YSensitivity = -1;
        [SerializeField]
        protected float smoothTime = 1.1f;

        private bool invertX = true;
        private bool invertY = true;
        
        /// <summary>
        /// Reference to all the cinemachine vcams on the character
        /// All the VCams must have the 'aim' set to POV 
        /// </summary>
        [SerializeField]
        protected CinemachineVirtualCamera[] VCams;	
        [SerializeField]
        protected CinemachinePOV mPOV; 

        private Vector2 look;

        private bool usingTouchControls;

        [SerializeField] 
        protected GameObject onScreenTouch;
        
        void Awake ()
        {
            mPOV = VCams[0].GetCinemachineComponent<CinemachinePOV>();
            
            // If onScreen touch controls are avtive, then switch off mouse look 
            if (onScreenTouch != null)
            {
                if (onScreenTouch.active)
                {
                    usingTouchControls = true;
                }
            }
            else
            {
                usingTouchControls = false;
            }
           
        }

        void Update()
        {
            ResponsiveMouseLook();
            if (!usingTouchControls)
            {
                look.x = Input.GetAxis(lookXAxisName);
                look.y = Input.GetAxis(lookYAxisName);
            }
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
        
        //Add missing methods 
        public void InvertXAxis()
        {
            invertX = !invertX;
            XSensitivity *= -1;
        }
        
        public void InvertYAxis()
        {
            invertY = !invertY;
            YSensitivity *= -1;
        }

        public void SetSensitivity(float x, float y)
        {
            if (x != 0)
            {
                if (invertX)
                {
                    x *= -1;
                }
                XSensitivity = x;
            }

            if (y != 0)
            {
                if (invertY)
                {
                    y *= -1;
                }
                YSensitivity = y;
            }
        }

        public Vector2 GetSensitivity()
        {
            return new Vector2(XSensitivity,YSensitivity);
        }
    }
}
using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace StandardAssets.Characters.FirstPerson
{
    /// <summary>
    /// This script is to update the various POV cams for 1st person
    /// If a player is in crouch mode, and looks around, the prone and walk camera positions will be updated.
    /// </summary>
    public class FirstPersonMouseLookPOVCamera: MonoBehaviour
    {     
        [SerializeField]
        protected CinemachineVirtualCameraBase vCamBase;
   
        /// <summary>
        /// Reference to all the cinemachine vcams on the character
        /// All the VCams must have the 'aim' set to POV 
        /// </summary>
        [SerializeField]
        protected CinemachineVirtualCamera[] VCams;	
       
        private CinemachinePOV currentLivePOVCam; 

        
        void Awake ()
        {
            SetCurrentLiveCamera();
        }

        void Update()
        {
            UpdatePovCameras();
        }
        
        /// <summary>
        /// Update each cameras Horizontal and Vertical axis values to the current live camera
        /// </summary>
        /// <param name="cameraRotation"></param>
        void UpdatePovCameras()
        {
            SetCurrentLiveCamera();
            
            var cameraRotation = new Vector3(currentLivePOVCam.m_VerticalAxis.Value, currentLivePOVCam.m_HorizontalAxis.Value, 0);
            
            foreach (var cam in VCams)
            {
                var vCam = cam.GetCinemachineComponent<CinemachinePOV>();
                vCam.m_HorizontalAxis.Value = cameraRotation.y;
                vCam.m_VerticalAxis.Value = cameraRotation.x;
            }
        }

        ///<summary>
        ///Set the current live camera
        ///<summary>
        void SetCurrentLiveCamera()
        {
            foreach(var cam in VCams)
            {
                if(vCamBase.IsLiveChild(cam))
                {
                    currentLivePOVCam = cam.GetCinemachineComponent<CinemachinePOV>();
                    break;
                }
            } 
        }
        
      
    }
}
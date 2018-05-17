using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
    /// <summary>
    /// Camera manager that listens to state changes on the IThirdPersonMotorStateMachine
    /// </summary>
    [RequireComponent(typeof(IThirdPersonMotorStateMachine))]
    public class ThirdPersonCameraManager:MonoBehaviour
    {
        IThirdPersonMotorStateMachine m_StateMachine;
        CinemachineVirtualCameraBase m_CurrentCamera;
        
        public CinemachineVirtualCameraBase closeCamera;
        public CinemachineVirtualCameraBase mediumCamera;
        public CinemachineVirtualCameraBase farCamera;
        
        
        /// <summary>
        /// Gets the state machine and subscribes to state change actions
        /// </summary>
        void Awake()
        {
            m_StateMachine = GetComponent<IThirdPersonMotorStateMachine>();
            m_StateMachine.idling += OnIdle;
            m_StateMachine.running += OnRun;
            m_StateMachine.walking += OnWalk;
            
            //SetDefaultCamera
            m_CurrentCamera = closeCamera;
        }

        /// <summary>
        /// Called on walk start
        /// </summary>
        void OnWalk()
        {
            SwitchCameras(mediumCamera);
        }

        /// <summary>
        /// Called on run start
        /// </summary>
        void OnRun()
        {
           
            SwitchCameras(farCamera);
        }

        /// <summary>
        /// Called on idle start
        /// </summary>
        void OnIdle()
        {
            
            SwitchCameras(closeCamera);
        }

        /// <summary>
        /// Helper for switching cameras
        /// </summary>
        /// <param name="newCamera"></param>
        void SwitchCameras(CinemachineVirtualCameraBase newCamera)
        {
            if (m_CurrentCamera != null)
            {
                m_CurrentCamera.Priority = 0;
            }
            
            m_CurrentCamera = newCamera;
            m_CurrentCamera.Priority = 100;
        }
    }
}
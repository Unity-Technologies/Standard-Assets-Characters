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
        GameObject m_CurrentCamera;
        
        public GameObject closeCamera;
        public GameObject mediumCamera;
        public GameObject farCamera;
        
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
        void SwitchCameras(GameObject newCamera)
        {
            m_CurrentCamera.gameObject.SetActive(false);
            m_CurrentCamera = newCamera;
            m_CurrentCamera.gameObject.SetActive(true);
        }
    }
}
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(IThirdPersonMotorStateMachine))]
    public class ThirdPersonCameraManager:MonoBehaviour
    {
        private IThirdPersonMotorStateMachine m_StateMachine;

        private GameObject currentCamera;
        public GameObject closeCamera;
        public GameObject mediumCamera;
        public GameObject farCamera;
        

        void Awake()
        {
            m_StateMachine = GetComponent<IThirdPersonMotorStateMachine>();
            m_StateMachine.idling += OnIdle;
            m_StateMachine.running += OnRun;
            m_StateMachine.walking += OnWalk;
            
            //SetDefaultCamera
            currentCamera = closeCamera;
        }

        private void OnWalk()
        {
            Debug.Log("THIS IS THE WALK CAMERA POSITION ");
            SwitchCameras(mediumCamera);

        }

        private void OnRun()
        {
            Debug.Log("THIS IS THE RUN CAMERA POSITION ");
            SwitchCameras(farCamera);
            
        }

        private void OnIdle()
        {
            Debug.Log("THIS IS THE IDLE CAMERA POSITION ");
            SwitchCameras(closeCamera);
        }

        void SwitchCameras(GameObject newCamera)
        {
            currentCamera.gameObject.SetActive(false);
            currentCamera = newCamera;
            currentCamera.gameObject.SetActive(true);
        }
        void Update()
        {
            //Do stuff with stateMachine Actions?
            
        }
    }
}
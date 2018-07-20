using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class CameraManager : MonoBehaviour
	{
		[SerializeField]
		protected ThirdPersonMotor motor;

		[SerializeField]
		protected GameObject strafeSDC, actionSDC;
		
		
		[SerializeField]
		protected CinemachineStateDrivenCamera actionCameraState;
		[SerializeField]
		protected CinemachineStateDrivenCamera strafeCameraState;

		private int livePriority = 10;
		private int standbyPriority = 1;
		
		[SerializeField]
		protected InputResponse recenterCamera;
	
		private void Awake()
		{
			recenterCamera.Init();
		}

		private void OnEnable()
		{
			if (motor == null)
			{
				Debug.LogError("Could not locate Third Person Motor - cannot manage cameras");
				return;
			}
			
			motor.startStrafeMode += MotorOnStartStrafeMode;
			motor.startActionMode += MotorOnStartActionMode;
			recenterCamera.started += RecenterCamera;
			
		}

		private void OnDisable()
		{
			if (motor == null)
			{
				return;
			}
			
			motor.startStrafeMode -= MotorOnStartStrafeMode;
			motor.startActionMode -= MotorOnStartActionMode;
			recenterCamera.started -= RecenterCamera;
		}
		
		void MotorOnStartActionMode()
		{
			 foreach (var cam in actionCameraState.ChildCameras)
			{
				cam.GetComponent<CinemachineFreeLook>().m_XAxis.Value = cam.transform.eulerAngles.y;
				cam.GetComponent<CinemachineFreeLook>().m_YAxis.Value =
					strafeCameraState.ChildCameras[0].GetComponent<CinemachineFreeLook>().m_YAxis.Value;
				//cam.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.DoRecentering();
			}
			 
			
			actionCameraState.Priority = livePriority;
            strafeCameraState.Priority = standbyPriority;
			//strafeSDC.SetActive(false);
			//actionSDC.SetActive(true);
		}

		void MotorOnStartStrafeMode()
		{	
			
			//TODO Get Strafe camera to start with the expected YAxis value
			
			actionCameraState.Priority = standbyPriority;
			strafeCameraState.Priority = livePriority;
			//strafeSDC.SetActive(true);
			//actionSDC.SetActive(false);
		}

		void RecenterCamera()
		{
			Debug.Log("Recenter Camera");
			
			Debug.Log(actionCameraState.ChildCameras[0].transform.eulerAngles.y);

		}
		
		
	}
}
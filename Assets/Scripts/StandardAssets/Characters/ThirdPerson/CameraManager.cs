using Cinemachine;
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

		private void OnEnable()
		{
			if (motor == null)
			{
				Debug.LogError("Could not locate Third Person Motor - cannot manage cameras");
				return;
			}
			
			motor.startStrafeMode += MotorOnStartStrafeMode;
			motor.startActionMode += MotorOnStartActionMode;
		}

		private void OnDisable()
		{
			if (motor == null)
			{
				return;
			}
			
			motor.startStrafeMode -= MotorOnStartStrafeMode;
			motor.startActionMode -= MotorOnStartActionMode;
		}
		
		void MotorOnStartActionMode()
		{
			foreach (var cam in actionCameraState.ChildCameras)
			{
				cam.GetComponent<CinemachineFreeLook>().m_XAxis.Value = 0;
				cam.GetComponent<CinemachineFreeLook>().m_YAxis.Value =
					strafeCameraState.ChildCameras[0].GetComponent<CinemachineFreeLook>().m_YAxis.Value;
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
	}
}
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// When on mobile, this will set the max speed for X and Y axis on the Cinemachine VCams
	/// to a lower value to make the camera easier to control. 
	/// </summary>
	public class MobileCameraSensitivityAdjustment : MonoBehaviour
	{
		/// <summary>
		/// Scale the max speed for each axis by this amount
		/// </summary>
		[SerializeField, Range(0.1f,1f), Tooltip("How much to scale camera speed on mobile")]
		protected float scaleMaxLookSpeedOnMobile = 0.75f;
		
		private CinemachineStateDrivenCamera stateDrivenCamera;

		/// <summary>
		/// Sets the camera speed if running on mobile platform
		/// </summary>
		private void Awake()
		{
#if UNITY_ANDROID || UNITY_IOS
			stateDrivenCamera = GetComponent<CinemachineStateDrivenCamera>();
			SetCameraSpeed();
#endif
		}
		
		/// <summary>
		/// Adjust the max speed for each axis of the cameras
		/// in the StateDrivenCamera for easier control for mobile
		/// </summary>
		private void SetCameraSpeed()
		{
			if (stateDrivenCamera != null)
			{
				foreach (CinemachineVirtualCameraBase childCamera in stateDrivenCamera.ChildCameras)
				{
					CinemachineVirtualCamera cinemachineVCam = childCamera.GetComponent<CinemachineVirtualCamera>();
					
					if (cinemachineVCam !=null) 
					{
						CinemachinePOV povCam = cinemachineVCam.GetCinemachineComponent<CinemachinePOV>();
						
						if (povCam != null)
						{
							povCam.m_HorizontalAxis.m_MaxSpeed *= scaleMaxLookSpeedOnMobile;
							povCam.m_VerticalAxis.m_MaxSpeed *= scaleMaxLookSpeedOnMobile;
						}
						continue;
					}

					CinemachineFreeLook cinemachineFreeLookCam = childCamera.GetComponent<CinemachineFreeLook>();
					
					if (cinemachineFreeLookCam != null)
					{
						cinemachineFreeLookCam.m_XAxis.m_MaxSpeed *= scaleMaxLookSpeedOnMobile;
						cinemachineFreeLookCam.m_YAxis.m_MaxSpeed *= scaleMaxLookSpeedOnMobile;
					}
				}
			}
		}
	}
}
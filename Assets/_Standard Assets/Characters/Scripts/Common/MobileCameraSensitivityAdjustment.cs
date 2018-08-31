using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.Common
{
	/// <summary>
	/// When on mobile, this will set the max speed for X and Y axis on the Cinemachine VCams
	/// to a lower value to make the look easier to control. 
	/// </summary>
	public class MobileCameraSensitivityAdjustment:MonoBehaviour
	{
		/// <summary>
		/// Slow the max speed for each axis by this amount
		/// </summary>
		[SerializeField]
		protected float maxSpeedDecreaseValue = 0.75f;

		[FormerlySerializedAs("stateCamera"),SerializeField]
		protected CinemachineStateDrivenCamera explorationStateCamera;

		[SerializeField]
		protected CinemachineFreeLook thirdPersonIdleCamera;

		[SerializeField]
		protected GameObject recenterButton;

		private void Awake()
		{
			
#if UNITY_ANDROID || UNITY_IOS
			SetCameraSpeed();
			
#endif			
		}

		void Update()
		{
			if (explorationStateCamera.IsLiveChild(thirdPersonIdleCamera))
			{
				recenterButton.SetActive(true);
			}
			else
			{
				recenterButton.SetActive(false);
			}
		}
	
		void SetCameraSpeed()
		{
			
			if (explorationStateCamera != null)
			{
				foreach (var childCamera in explorationStateCamera.ChildCameras)
				{
					var cinemachineVCam = childCamera.GetComponent<CinemachineVirtualCamera>();
					
					if (cinemachineVCam !=null)
					{
						var povCam = cinemachineVCam.GetCinemachineComponent<CinemachinePOV>();
						
						if (povCam != null)
						{
							povCam.m_HorizontalAxis.m_MaxSpeed *= maxSpeedDecreaseValue;
							povCam.m_VerticalAxis.m_MaxSpeed *= maxSpeedDecreaseValue;
						}
						
						continue;
					}

					var cinemachineFreeLookCam = childCamera.GetComponent<CinemachineFreeLook>();
					if (cinemachineFreeLookCam != null)
					{
						cinemachineFreeLookCam.m_XAxis.m_MaxSpeed *= maxSpeedDecreaseValue;
						cinemachineFreeLookCam.m_YAxis.m_MaxSpeed *= maxSpeedDecreaseValue;
					}
				}
			}
			
		}
	}
}
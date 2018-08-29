using System;
using Cinemachine;
using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonCameraAnimationManager : CameraAnimationManager
	{
		public event Action forwardUnlockedModeStarted, forwardLockedModeStarted;

		[SerializeField]
		protected ThirdPersonBrain brain;
		
		[DisableEditAtRuntime()]
		[SerializeField]
		protected ThirdPersonCameraType startingCameraMode = ThirdPersonCameraType.Exploration;

		[SerializeField]
		protected InputResponse cameraModeInput, cameraToggleInput;

		[SerializeField]
		protected string[] explorationCameraStates, strafeCameraStates;

		[SerializeField]
		protected GameObject[] explorationCameraObjects, strafeCameraObjects;

		[SerializeField]
		protected CinemachineStateDrivenCamera explorationStateDrivenCamera;

		[SerializeField]
		protected CinemachineStateDrivenCamera strafeStateDrivenCamera;
		
		private string[] currentCameraModeStateNames;

		private int cameraIndex;

		private bool isForwardUnlocked;

		private CinemachineStateDrivenCamera thirdPersonStateDrivenCamera;

		private bool isChangingMode;


		private void Awake()
		{
			thirdPersonStateDrivenCamera = GetComponent<CinemachineStateDrivenCamera>();

			

			if (cameraModeInput != null)
			{
				cameraModeInput.Init();
			}

			if (cameraToggleInput != null)
			{
				cameraToggleInput.Init();
			}
		}

		private void Start()
		{
			if (brain != null)
			{
				brain.CurrentMotor.landed += OnLanded;
			}
			
			isForwardUnlocked = startingCameraMode == ThirdPersonCameraType.Exploration;
			SetForwardModeArray();
			SetAnimation(currentCameraModeStateNames[cameraIndex]);
			PlayForwardModeEvent();
		}

		private void OnEnable()
		{
			if (cameraModeInput != null)
			{
				cameraModeInput.started += ChangeCameraMode;
				cameraModeInput.ended += ChangeCameraMode;
			}

			if (cameraToggleInput != null)
			{
				cameraToggleInput.started += ChangeCameraToggle;
				cameraToggleInput.ended += ChangeCameraToggle;
			}
		}

		private void OnDisable()
		{
			if (cameraModeInput != null)
			{
				cameraModeInput.started -= ChangeCameraMode;
				cameraModeInput.ended -= ChangeCameraMode;
			}

			if (cameraToggleInput != null)
			{
				cameraToggleInput.started -= ChangeCameraToggle;
				cameraToggleInput.ended -= ChangeCameraToggle;
			}
		}

		private void ChangeCameraToggle()
		{
			SetCameraState();
		}

		private void ChangeCameraMode()
		{
			isChangingMode = true;

			if (brain.physicsForCharacter != null && brain.physicsForCharacter.isGrounded)
			{
				PerformCameraModeChange();
			}
		}
		
		private void OnLanded()
		{
			if (isChangingMode)
			{
				PerformCameraModeChange();
			}
		}

		private void PerformCameraModeChange()
		{
			isForwardUnlocked = !isForwardUnlocked;
			SetForwardModeArray();
			cameraIndex = -1;
			SetCameraState();
			PlayForwardModeEvent();
			isChangingMode = false;
		}

		private void SetForwardModeArray()
		{
			currentCameraModeStateNames = isForwardUnlocked
				? explorationCameraStates
				: strafeCameraStates;
		}

		private void PlayForwardModeEvent()
		{
			if (isForwardUnlocked)
			{
				SetCameraObjectsActive(explorationCameraObjects);

				SetCameraObjectsActive(strafeCameraObjects, false);

				if (forwardUnlockedModeStarted != null)
				{
					forwardUnlockedModeStarted();
				}
			}
			else
			{
				SetCameraObjectsActive(explorationCameraObjects, false);

				if (forwardLockedModeStarted != null)
				{
					forwardLockedModeStarted();
				}
			}
		}

		//TEMP TO SWITCH CROSSHAIR ON ONCE BLEND IS FINISHED 
		private void Update()
		{
			if (!isForwardUnlocked)
			{
				if (!thirdPersonStateDrivenCamera.IsBlending)
				{
					SetCameraObjectsActive(strafeCameraObjects);
				}
			}
		}

		private void SetCameraState()
		{
			cameraIndex++;
			if (cameraIndex >= currentCameraModeStateNames.Length)
			{
				cameraIndex = 0;
			}

			if (isForwardUnlocked)
			{
				SetCameraAxes(strafeStateDrivenCamera, explorationStateDrivenCamera);
			}
			else
			{
				SetCameraAxes(explorationStateDrivenCamera, strafeStateDrivenCamera);
			}

			SetAnimation(currentCameraModeStateNames[cameraIndex]);
		}

		private void SetCameraAxes(CinemachineStateDrivenCamera sourceStateDrivenCamera,
		                           CinemachineStateDrivenCamera destinationStateDrivenCamera)
		{
			foreach (CinemachineVirtualCameraBase camera in sourceStateDrivenCamera.ChildCameras)
			{
				if (sourceStateDrivenCamera.IsLiveChild(camera))
				{
					float cameraX = camera.GetComponent<CinemachineFreeLook>().m_XAxis.Value;
					float cameraY = camera.GetComponent<CinemachineFreeLook>().m_YAxis.Value;
					SetChildCamerasAxis(destinationStateDrivenCamera, cameraX, cameraY);
				}
			}
		}

		private void SetChildCamerasAxis(CinemachineStateDrivenCamera stateDrivenCamera, float xAxis, float yAxis)
		{
			foreach (CinemachineVirtualCameraBase childCamera in stateDrivenCamera.ChildCameras)
			{
				childCamera.GetComponent<CinemachineFreeLook>().m_XAxis.Value = xAxis;
				childCamera.GetComponent<CinemachineFreeLook>().m_YAxis.Value = yAxis;
			}
		}

		private void SetCameraObjectsActive(GameObject[] cameraObjects, bool isActive = true)
		{
			foreach (GameObject cameraObject in cameraObjects)
			{
				cameraObject.SetActive(isActive);
			}
		}
	}

	public enum ThirdPersonCameraType
	{
		Exploration,
		Strafe
	}
}
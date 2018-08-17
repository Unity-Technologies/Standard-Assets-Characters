using System;
using Attributes;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonCameraAnimationManager : CameraAnimationManager
	{
		public event Action forwardUnlockedModeStarted, forwardLockedModeStarted;

		[DisableAtRuntime()]
		[SerializeField]
		protected ThirdPersonCameraType startingCameraMode = ThirdPersonCameraType.Exploration;
		
		[SerializeField]
		protected InputResponse cameraModeInput, cameraToggleInput;

		[SerializeField]
		protected string[] explorationCameraStates, strafeCameraStates;

		[SerializeField]
		protected GameObject[] explorationCameraObjects, strafeCameraObjects;

		private string[] currentCameraModeStateNames;

		private int cameraIndex;

		private bool isForwardUnlocked;

		private CinemachineStateDrivenCamera thirdPersonStateDrivenCamera;

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
			isForwardUnlocked = !isForwardUnlocked;		
			SetForwardModeArray();
			cameraIndex = -1;
			SetCameraState();
			PlayForwardModeEvent();
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
			
			SetAnimation(currentCameraModeStateNames[cameraIndex]);
			
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
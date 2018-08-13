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
		
		[SerializeField]
		protected bool defaultModeIsForwardUnlocked = true;

		[SerializeField]
		protected InputResponse cameraModeInput, cameraToggleInput;

		[SerializeField]
		protected string[] forwardUnlockedCameraToggleStateNames, forwardLockedCameraToggleStateNames;

		private string[] currentCameraModeStateNames;

		private int cameraIndex;

		private bool isForwardUnlocked;

		private void Awake()
		{
			if (cameraModeInput != null)
			{
				cameraModeInput.Init();
			}

			if (cameraToggleInput != null)
			{
				cameraToggleInput.Init();
			}
			
			isForwardUnlocked = defaultModeIsForwardUnlocked;
			SetForwardModeArray();
			SetAnimation(currentCameraModeStateNames[cameraIndex]);
			PlayForwardModeEvent();
		}

		private void OnEnable()
		{
			cameraModeInput.started += ChangeCameraMode;
			cameraModeInput.ended += ChangeCameraMode;
			cameraToggleInput.started += ChangeCameraToggle;
			cameraToggleInput.ended += ChangeCameraToggle;
		}
		
		private void OnDisable()
		{
			cameraModeInput.started -= ChangeCameraMode;
			cameraModeInput.ended -= ChangeCameraMode;
			cameraToggleInput.started -= ChangeCameraToggle;
			cameraToggleInput.ended -= ChangeCameraToggle;
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
				? forwardUnlockedCameraToggleStateNames
				: forwardLockedCameraToggleStateNames;
		}

		private void PlayForwardModeEvent()
		{
			if (isForwardUnlocked)
			{
				if (forwardUnlockedModeStarted != null)
				{
					forwardUnlockedModeStarted();
				}
			}
			else
			{
				if (forwardLockedModeStarted != null)
				{
					forwardLockedModeStarted();
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
	}
}
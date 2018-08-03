using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.Common
{
	public class ThirdPersonCameraAnimationManager: CameraAnimationManager
	{
		private string[] actionCameraMode = {"Action Mode 1", "Action Mode 2"};
		private int currentActionModeIndex = 0;

		private string strafeState = "Strafe";
		
		[SerializeField]
		protected InputResponse changeCameraModeInputResponse;

		[SerializeField]
		protected InputResponse recenterCameraInputResponse;

		[SerializeField]
		protected LegacyCharacterInput characterInput;

		[SerializeField]
		protected CinemachineStateDrivenCamera actionStateDrivenCameraOne;
		
		[SerializeField]
		protected Text currentCameraText;
		
		
		private void Awake()
		{
			
			//currentCameraText.text = actionCameraMode[currentActionModeIndex];
			
			changeCameraModeInputResponse.Init();
			recenterCameraInputResponse.Init();
			
		}

		private void OnEnable()
		{
			changeCameraModeInputResponse.started += SwitchActionMode;
			recenterCameraInputResponse.started += RecenterCamera;

		}

		private void OnDisable()
		{
			changeCameraModeInputResponse.started -= SwitchActionMode;
			recenterCameraInputResponse.started -= RecenterCamera;
			
		}

		void RecenterCamera()
		{
			SetChildrenToRecenter(actionStateDrivenCameraOne);
		}

		private void Update()
		{ 
			//TODO
			//Add threshold for slight movements 
			
			//TODO
			//Add in recenter for run and idle cameras 
			if (characterInput.hasMovementInput
			    |characterInput.lookInput != Vector2.zero)
			{
				UnsetChildrenToRecenter(actionStateDrivenCameraOne);
			}
	}

		public void StrafeStarted()
		{
			SetAnimation(strafeState);
			currentCameraText.text = strafeState;
		}

		public void StrafeEnded()
		{
			SetAnimation(actionCameraMode[currentActionModeIndex]);
			currentCameraText.text = actionCameraMode[currentActionModeIndex];
		}

		void SetChildrenToRecenter(CinemachineStateDrivenCamera stateDrivenCamera)
		{
			foreach (var childCamera in stateDrivenCamera.ChildCameras)
			{
				childCamera.GetComponentInChildren<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = true;
				childCamera.GetComponentInChildren<CinemachineFreeLook>().m_YAxisRecentering.m_enabled = true;
			}
		}

		void UnsetChildrenToRecenter(CinemachineStateDrivenCamera stateDrivenCamera)
		{
			foreach (var childCamera in stateDrivenCamera.ChildCameras)
			{
				childCamera.GetComponentInChildren<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = false;
				childCamera.GetComponentInChildren<CinemachineFreeLook>().m_YAxisRecentering.m_enabled = false;
			}
		}
		
		
		void SwitchActionMode()
		{
			if (currentActionModeIndex == 0)
			{
				currentActionModeIndex = 1;
			}
			else if (currentActionModeIndex == 1)
			{
				currentActionModeIndex = 0;
			}

			currentCameraText.text = actionCameraMode[currentActionModeIndex];
			SetAnimation(actionCameraMode[currentActionModeIndex]);
		}

	}
}
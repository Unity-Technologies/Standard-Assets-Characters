using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.Common
{
	public class ThirdPersonCameraAnimationManager : CameraAnimationManager
	{
		private string[] actionCameraMode = {"World Camera", "FreeLook Camera", "Hyrbid Camera"};
		private int currentCameraModeIndex = 0;
		private string strafeStateName = "Strafe";

		[SerializeField]
		protected InputResponse changeCameraModeInputResponse;

		[SerializeField]
		protected InputResponse recenterCameraInputResponse;

		[SerializeField]
		protected LegacyCharacterInput characterInput;

		[SerializeField]
		protected Text currentCameraText;

		[SerializeField]
		protected CinemachineFreeLook hybridIdleCamera;

		[SerializeField]
		protected CameraAnimationManager cameraAnimationManager;

		[SerializeField]
		protected CinemachineFreeLook defaultWorldFreelookCam;

		private bool recetnerHybridIdle;


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
			
			//Recenter default world camera freelook if character is not moving
			if (!characterInput.hasMovementInput)
			{
				RecenterFreeLookCam(defaultWorldFreelookCam);
			}
			
			//For hybrid camera, if the player is moving then the camera will change
			//To Follow mode. 
			if (characterInput.hasMovementInput)
			{
				cameraAnimationManager.SetAnimation("Follow",1);
			}
			else
			{
				//if the character is not moving, and the state is in world, and the player presses recenter
				//The camera should recenter and return to world state. 
				if (cameraAnimationManager.ActiveState("Follow", 1))
				{
					cameraAnimationManager.SetAnimation("World",1);
				}
				RecenterFreeLookCam(hybridIdleCamera);
				recetnerHybridIdle = true;
			}
		}

		private void Update()
		{
			if (recetnerHybridIdle && characterInput.hasMovementInput)
			{
				cameraAnimationManager.SetAnimation("Follow",1);
			}
			
			//Idle cameras will turn off recenter if there is any movement on left or 
			//Right sticks. 
			if (characterInput.hasMovementInput
			    | characterInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(hybridIdleCamera);
				recetnerHybridIdle = false;
				
				TurnOffFreeLookCamRecenter(defaultWorldFreelookCam);
			}

			//The run cam will turn off recenter only if there is movmement on the right stick (look)
			// In the case of hybrid camera (running) 
			if (characterInput.lookInput.x > 0.1 || characterInput.lookInput.x < -0.1)
			{
				cameraAnimationManager.SetAnimation("World", 1);
			}
		}

		public void StrafeStarted()
		{
			SetAnimation(strafeStateName);
			currentCameraText.text = strafeStateName;
		}

		public void StrafeEnded()
		{
			SetAnimation(actionCameraMode[currentCameraModeIndex]);
			currentCameraText.text = actionCameraMode[currentCameraModeIndex];
		}

		/// <summary>
		/// Sets the given Freelook Cinemachine camera
		/// to recenter for X/Y axis On/Off
		/// </summary>
		/// <param name="freeLook"></param>
		void RecenterFreeLookCam(CinemachineFreeLook freeLook)
		{
			freeLook.m_RecenterToTargetHeading.m_enabled = true;
			freeLook.m_YAxisRecentering.m_enabled = true;
		}

		void TurnOffFreeLookCamRecenter(CinemachineFreeLook freeLook)
		{
			freeLook.m_RecenterToTargetHeading.m_enabled = false;
			freeLook.m_YAxisRecentering.m_enabled = false;
		}

		void SwitchActionMode()
		{
			//Index 0 - WorldCamera Mode
			//Index 1 - FreelookCamera Mode
			//Index 2 - HybridCamera Mode
			if (currentCameraModeIndex == 0 || currentCameraModeIndex == 1)
			{
				currentCameraModeIndex++;
			}
			else if (currentCameraModeIndex == 2)
			{
				currentCameraModeIndex = 0;
			}

			currentCameraText.text = actionCameraMode[currentCameraModeIndex];
			
			SetAnimation(actionCameraMode[currentCameraModeIndex]);
		}
	}
}
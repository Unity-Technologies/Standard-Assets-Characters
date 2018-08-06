using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.Common
{
	public class ThirdPersonCameraAnimationManager : CameraAnimationManager
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

		//[SerializeField]
		//protected CinemachineStateDrivenCamera actionStateDrivenCameraOne;

		[SerializeField]
		protected Text currentCameraText;

		[SerializeField]
		protected CinemachineFreeLook runFreelook;

		[SerializeField]
		protected CinemachineFreeLook idleFreelook;


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

			changeCameraModeInputResponse.ended += TestEdnded;

		}

		private void OnDisable()
		{
			changeCameraModeInputResponse.started -= SwitchActionMode;
			recenterCameraInputResponse.started -= RecenterCamera;

		}

		void RecenterCamera()
		{
			//SetChildrenToRecenter(actionStateDrivenCameraOne);
			RecenterFreeLookCam(idleFreelook);
			RecenterFreeLookCam(runFreelook);
		}

		private void Update()
		{
			//TODO
			//Add threshold for slight movements 

			//TODO
			//Add in recenter for run and idle cameras 

			//The idle cam will turn off recenter if there is any movement on left or 
			//Right sticks. 
			if (characterInput.hasMovementInput
			    | characterInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(idleFreelook);
				//UnsetChildrenToRecenter(actionStateDrivenCameraOne);
			}

			//The run cam will turn off recenter only if there is movmement on the right stick (look)
			if (characterInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(runFreelook);
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

		/// <summary>
		/// Sets the given Freelook Cinemachine camera
		/// to recenter for X/Y axis
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

		void TestEdnded()
		{
			Debug.Log("Ended");
		}

	void SwitchActionMode()
		{
			Debug.Log("Switch Camer aMode");
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
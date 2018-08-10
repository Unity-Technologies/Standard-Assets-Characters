using System;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.Common
{
	public class ThirdPersonCameraAnimationManager : CameraAnimationManager
	{
		private string strafeStateName = "Strafe";

		[SerializeField]
		protected InputResponse recenterCameraInputResponse;

		[SerializeField]
		protected LegacyCharacterInput characterInput;

		[SerializeField]
		protected Text currentCameraText;

		[SerializeField]
		protected CinemachineFreeLook defaultWorldFreelookCam;

		private bool recetnerHybridIdle;


		private void Awake()
		{
			
			recenterCameraInputResponse.Init();
		}

		private void OnEnable()
		{
			recenterCameraInputResponse.started += RecenterCamera;
		}

		private void OnDisable()
		{
			recenterCameraInputResponse.started -= RecenterCamera;
		}

		void RecenterCamera()
		{
			
			//Recenter default world camera freelook if character is not moving
			if (!characterInput.hasMovementInput)
			{
				RecenterFreeLookCam(defaultWorldFreelookCam);
			}
			
		}

		private void Update()
		{
			//Idle cameras will turn off recenter if there is any movement on left or 
			//Right sticks. 
			if (characterInput.hasMovementInput
			    | characterInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(defaultWorldFreelookCam);
			}			
	}

		public void StrafeStarted()
		{
			SetAnimation(strafeStateName);
			currentCameraText.text = strafeStateName;
		}

		public void StrafeEnded()
		{
			SetAnimation("World Camera");
			currentCameraText.text = "World Camera";
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

	}
}
using System;
using Cinemachine;
using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Implementation of <see cref="CameraAnimationManager"/> to manage third person camera states 
	/// </summary>
	public class ThirdPersonCameraAnimationManager : CameraAnimationManager
	{
		public event Action forwardUnlockedModeStarted, forwardLockedModeStarted;

		[SerializeField, Tooltip("Third person character brain")]
		protected ThirdPersonBrain brain;
		
		[DisableEditAtRuntime()]
		[SerializeField]
		protected ThirdPersonCameraType startingCameraMode = ThirdPersonCameraType.Exploration;

		[SerializeField, Tooltip("Input Response for changing camera mode and camera recenter")]
		protected InputResponse cameraModeInput, recenterCameraInput;

		[SerializeField, Tooltip("Legacy on screen character input")]
		protected LegacyOnScreenCharacterInput mobileCharacterInput;
		
		[SerializeField, Tooltip("Legacy stand alone character input")]
		protected LegacyCharacterInput standAloneCharacterInput;

		[SerializeField, Tooltip("State Driven Camera state names")]
		protected string[] explorationCameraStates, strafeCameraStates;

		[SerializeField, Tooltip("Game objects to toggle when switching camera modes")]
		protected GameObject[] explorationCameraObjects, strafeCameraObjects;

		[SerializeField, Tooltip("Cinemachine State Driven Camera")]
		protected CinemachineStateDrivenCamera explorationStateDrivenCamera;

		[SerializeField, Tooltip("Cinemachine State Driven Camera")]
		protected CinemachineStateDrivenCamera strafeStateDrivenCamera;
		
		[SerializeField, Tooltip("This is the free look camera that will be able to get recentered")]
		protected CinemachineFreeLook idleCamera;
		
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
			
			if (recenterCameraInput != null)
			{
				recenterCameraInput.Init();
			}
		}

		protected override void Start()
		{
			base.Start();
			
			if (brain != null)
			{
				brain.currentMotor.landed += OnLanded;
			}
			
			isForwardUnlocked = startingCameraMode == ThirdPersonCameraType.Exploration;
			SetForwardModeArray();
			SetAnimation(currentCameraModeStateNames[cameraIndex]);
			PlayForwardModeEvent();
		}
		
		/// <summary>
		/// Subscribe to input events 
		/// </summary>
		private void OnEnable()
		{
			if (cameraModeInput != null)
			{
				cameraModeInput.started += ChangeCameraMode;
				cameraModeInput.ended += ChangeCameraMode;
			}
			
			if (recenterCameraInput != null)
			{
				recenterCameraInput.started += RecenterCamera;
				recenterCameraInput.ended += RecenterCamera;
			}
		}
		
		/// <summary>
		/// Unsubscribe from input events
		/// </summary>
		private void OnDisable()
		{
			if (cameraModeInput != null)
			{
				cameraModeInput.started -= ChangeCameraMode;
				cameraModeInput.ended -= ChangeCameraMode;
			}
			
			if (recenterCameraInput != null)
			{
				recenterCameraInput.started -= RecenterCamera;
				recenterCameraInput.ended -= RecenterCamera;
			}
		}
		
		private void ChangeCameraMode()
		{
			isChangingMode = true;

			if (brain.physicsForCharacter != null && brain.physicsForCharacter.isGrounded)
			{
				PerformCameraModeChange();
			}
		}
		
		private void RecenterCamera()
		{
#if UNITY_ANDROID || UNITY_IOS
			if (!mobileCharacterInput.hasMovementInput)
			{
				RecenterFreeLookCam(idleCamera);
			}
#endif		
			if (!standAloneCharacterInput.hasMovementInput)
			{
				RecenterFreeLookCam(idleCamera);
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
		
		private void Update()
		{
			if (!isForwardUnlocked)
			{
				if (!thirdPersonStateDrivenCamera.IsBlending)
				{
					SetCameraObjectsActive(strafeCameraObjects);
				}
			}
			
#if UNITY_ANDROID || UNITY_IOS
			if (mobileCharacterInput.hasMovementInput
			    || mobileCharacterInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(idleCamera);
			}		
#endif		
			if (standAloneCharacterInput.hasMovementInput
			    || standAloneCharacterInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(idleCamera);
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
		
		private void RecenterFreeLookCam(CinemachineFreeLook freeLook)
		{
			freeLook.m_RecenterToTargetHeading.m_enabled = true;
			freeLook.m_YAxisRecentering.m_enabled = true;
		}

		private void TurnOffFreeLookCamRecenter(CinemachineFreeLook freeLook)
		{
			freeLook.m_RecenterToTargetHeading.m_enabled = false;
			freeLook.m_YAxisRecentering.m_enabled = false;
		}
		
		/// <summary>
		/// Keep virtual camera children of a state driven camera all
		/// pointing in the same direction when changing between state driven cameras
		/// </summary>
		/// <param name="sourceStateDrivenCamera">The state driven camera that is being transitioned from</param>
		/// <param name="destinationStateDrivenCamera">The state driven camera that is being transitioned to</param>
		private void SetCameraAxes(CinemachineStateDrivenCamera sourceStateDrivenCamera,
		                           CinemachineStateDrivenCamera destinationStateDrivenCamera)
		{
			foreach (CinemachineVirtualCameraBase camera in sourceStateDrivenCamera.ChildCameras)
			{
				if (sourceStateDrivenCamera.IsLiveChild(camera))
				{
					float cameraX = camera.GetComponent<CinemachineFreeLook>().m_XAxis.Value;
					float cameraY = camera.GetComponent<CinemachineFreeLook>().m_YAxis.Value;
					SetChildCameraAxes(destinationStateDrivenCamera, cameraX, cameraY);
				}
			}
		}
		
		private void SetChildCameraAxes(CinemachineStateDrivenCamera stateDrivenCamera, float xAxis, float yAxis)
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
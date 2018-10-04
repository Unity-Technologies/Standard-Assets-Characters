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
	/// Implementation of <see cref="CameraController"/> to manage third person camera states 
	/// </summary>
	public class ThirdPersonCameraController : CameraController
	{
		/// <summary>
		/// Enum used to describe third person camera type.
		/// </summary>
		protected enum CameraType
		{
			Exploration,
			Strafe
		}
		
		public event Action forwardUnlockedModeStarted, forwardLockedModeStarted;
		
		[DisableEditAtRuntime(), SerializeField, Tooltip("Define the starting camera mode")]
		protected CameraType startingCameraMode = CameraType.Exploration;

//		[SerializeField, Tooltip("Input Response for changing camera mode and camera recenter")]
//		protected InputResponse cameraModeInput, recenterCameraInput;

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
		
		private ThirdPersonBrain thirdPersonBrain;

		/// <inheritdoc/>
		protected override void Start()
		{
			base.Start();
			
			isForwardUnlocked = startingCameraMode == CameraType.Exploration;
			SetForwardModeArray();
			SetAnimation(currentCameraModeStateNames[cameraIndex]);
			PlayForwardModeEvent();
		}
		
		private void Awake()
		{
			thirdPersonStateDrivenCamera = GetComponent<CinemachineStateDrivenCamera>();
//			if (cameraModeInput != null)
//			{
//				cameraModeInput.Init();
//			}		
//			if (recenterCameraInput != null)
//			{
//				recenterCameraInput.Init();
//			}
		}
		
		/// <summary>
		/// Subscribe to input and <see cref="IThirdPersonMotor.landed"/> events.
		/// </summary>
		private void OnEnable()
		{
//			if (cameraModeInput != null)
//			{
//				cameraModeInput.started += ChangeCameraMode;
//				cameraModeInput.ended += ChangeCameraMode;
//			}
//			
//			if (recenterCameraInput != null)
//			{
//				recenterCameraInput.started += RecenterCamera;
//				recenterCameraInput.ended += RecenterCamera;
//			}
			
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.thirdPersonMotor.landed += OnLanded;
			}
		}
		
		/// <summary>
		/// Unsubscribe from input and <see cref="IThirdPersonMotor.landed"/> events.
		/// </summary>
		private void OnDisable()
		{
//			if (cameraModeInput != null)
//			{
//				cameraModeInput.started -= ChangeCameraMode;
//				cameraModeInput.ended -= ChangeCameraMode;
//			}
//			
//			if (recenterCameraInput != null)
//			{
//				recenterCameraInput.started -= RecenterCamera;
//				recenterCameraInput.ended -= RecenterCamera;
//			}
			
			if (thirdPersonBrain != null)
			{
				thirdPersonBrain.thirdPersonMotor.landed -= OnLanded;
			}
		}
		
		private void ChangeCameraMode()
		{
			isChangingMode = true;

			if (thirdPersonBrain.physicsForCharacter != null && thirdPersonBrain.physicsForCharacter.isGrounded)
			{
				PerformCameraModeChange();
			}
		}
		
		private void RecenterCamera()
		{
//			if (!thirdPersonBrain.inputForCharacter.hasMovementInput)
//			{
//				RecenterFreeLookCam(idleCamera);
//			}
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
			if (thirdPersonBrain == null)
			{
				Debug.LogError("No Third Person Brain in the scene", gameObject);
				gameObject.SetActive(false);
				return;
			}
			
			if (!isForwardUnlocked)
			{
				if (!thirdPersonStateDrivenCamera.IsBlending)
				{
					SetCameraObjectsActive(strafeCameraObjects);
				}
			}

//			if (thirdPersonBrain.inputForCharacter.hasMovementInput ||
//			    thirdPersonBrain.inputForCharacter.lookInput != Vector2.zero)
//			{
//				TurnOffFreeLookCamRecenter(idleCamera);
//			}	
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

		/// <summary>
		/// Sets the <see cref="ThirdPersonBrain"/> and automatically sets up the required fields for the Cinemachine cameras
		/// </summary>
		/// <param name="brainToUse">The third person brain to use</param>
		public void SetThirdPersonBrain(ThirdPersonBrain brainToUse)
		{
			thirdPersonBrain = brainToUse;

			//Automatically handle Cinemachine setup
			if (strafeStateDrivenCamera.m_AnimatedTarget == null)
			{
				strafeStateDrivenCamera.m_AnimatedTarget = thirdPersonBrain.GetComponent<Animator>();
			}
			
			if (explorationStateDrivenCamera.m_AnimatedTarget == null)
			{
				explorationStateDrivenCamera.m_AnimatedTarget = thirdPersonBrain.GetComponent<Animator>();
			}

			CinemachineStateDrivenCamera rootSdc = GetComponent<CinemachineStateDrivenCamera>();
			if (rootSdc != null)
			{
				rootSdc.m_LookAt = thirdPersonBrain.transform;
				rootSdc.m_Follow = thirdPersonBrain.transform;
			}
		}
	}
}
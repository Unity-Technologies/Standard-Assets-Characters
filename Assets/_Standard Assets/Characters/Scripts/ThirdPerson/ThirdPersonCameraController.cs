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
		private string k_ExplorationState = "Exploration", k_StrafeState = "Strafe";
		
		/// <summary>
		/// Enum used to describe third person camera type.
		/// </summary>
		protected enum CameraType
		{
			Exploration,
			Strafe
		}
		
		[DisableEditAtRuntime(), SerializeField, Tooltip("Define the starting camera mode")]
		protected CameraType startingCameraMode = CameraType.Exploration;

		[SerializeField, Tooltip("Game objects to toggle when switching camera modes")]
		protected GameObject[] explorationCameraObjects, strafeCameraObjects;

		[SerializeField, Tooltip("Cinemachine State Driven Camera")]
		protected CinemachineStateDrivenCamera explorationStateDrivenCamera;

		[SerializeField, Tooltip("Cinemachine State Driven Camera")]
		protected CinemachineStateDrivenCamera strafeStateDrivenCamera;
		
		[SerializeField, Tooltip("This is the free look camera that will be able to get recentered")]
		protected CinemachineFreeLook idleCamera;
		

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
			if (isForwardUnlocked)
			{
				SetAnimation(k_ExplorationState);
			}
			else
			{
				SetAnimation(k_StrafeState);
			}
			PlayForwardModeEvent();
		}
		
		private void Awake()
		{
			thirdPersonStateDrivenCamera = GetComponent<CinemachineStateDrivenCamera>();
		}
		
		/// <summary>
		/// Subscribe to input and <see cref="IThirdPersonMotor.landed"/> events.
		/// </summary>
		private void OnEnable()
		{
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
			if (!thirdPersonBrain.thirdPersonInput.hasMovementInput)
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
			cameraIndex = -1;
			if (isForwardUnlocked)
			{
				SetCameraAxes(strafeStateDrivenCamera, explorationStateDrivenCamera);
				SetAnimation(k_ExplorationState);
			}
			else
			{
				SetCameraAxes(explorationStateDrivenCamera, strafeStateDrivenCamera);
				SetAnimation(k_StrafeState);
			}
			PlayForwardModeEvent();
			isChangingMode = false;
		}

		private void PlayForwardModeEvent()
		{
			if (isForwardUnlocked)
			{
				SetCameraObjectsActive(explorationCameraObjects);
				SetCameraObjectsActive(strafeCameraObjects, false);
			}
			else
			{
				SetCameraObjectsActive(explorationCameraObjects, false);
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

			if (thirdPersonBrain.thirdPersonInput.hasMovementInput ||
			    thirdPersonBrain.thirdPersonInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(idleCamera);
			}	
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

		public void SetStrafeCamera()
		{
			PerformCameraModeChange();
		}

		public void SetExplorationCamera()
		{
			PerformCameraModeChange();
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
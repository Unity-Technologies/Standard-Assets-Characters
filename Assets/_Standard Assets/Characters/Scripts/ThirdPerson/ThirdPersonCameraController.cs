using System;
using Cinemachine;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Manages third person camera states 
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCameraController : MonoBehaviour
	{
		string m_ExplorationState = "Exploration", m_StrafeState = "Strafe";

		/// <summary>
		/// Enum used to describe third person camera type.
		/// </summary>
		enum CameraType
		{
			Exploration,
			Strafe
		}

		[FormerlySerializedAs("startingCameraMode")]
		[SerializeField, Tooltip("Define the starting camera mode")]
		CameraType m_StartingCameraMode = CameraType.Exploration;

		[FormerlySerializedAs("explorationStateDrivenCamera")]
		[SerializeField, Tooltip("Cinemachine State Driven Camera")]
		CinemachineStateDrivenCamera m_ExplorationStateDrivenCamera;

		[FormerlySerializedAs("strafeStateDrivenCamera")]
		[SerializeField, Tooltip("Cinemachine State Driven Camera")]
		CinemachineStateDrivenCamera m_StrafeStateDrivenCamera;

		[FormerlySerializedAs("idleCamera")]
		[SerializeField, Tooltip("This is the free look camera that will be able to get recentered")]
		CinemachineFreeLook m_IdleCamera;

		[FormerlySerializedAs("crosshair")]
		[SerializeField, Tooltip("The aiming crosshair that is visible during strafe")]
		GameObject m_Crosshair;

		ThirdPersonBrain m_ThirdPersonBrain;

		Animator m_Animator;

		/// <summary>
		/// Sets the animation to the defined state
		/// </summary>
		/// <param name="state">the name of the animation state</param>
		/// <param name="layer">the layer that the animation state is on</param>
		public void SetAnimation(string state, int layer = 0)
		{
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
			}
			m_Animator.Play(state,layer);
		}

		public void RecenterCamera()
		{
			if (!m_ThirdPersonBrain.thirdPersonInput.hasMovementInput)
			{
				RecenterFreeLookCam(m_IdleCamera);
			}
		}

		void Start()
		{
			if (m_StartingCameraMode == CameraType.Exploration)
			{
				SetAnimation(m_ExplorationState);
				SetCrosshairVisible(false);
				
			}
			else
			{
				SetAnimation(m_StrafeState);
				SetCrosshairVisible();
			}
		}

		void Update()
		{
			if (m_ThirdPersonBrain == null)
			{
				Debug.LogError("No Third Person Brain in the scene", gameObject);
				gameObject.SetActive(false);
				return;
			}

			if (m_ThirdPersonBrain.thirdPersonInput.hasMovementInput ||
			    m_ThirdPersonBrain.thirdPersonInput.lookInput != Vector2.zero)
			{
				TurnOffFreeLookCamRecenter(m_IdleCamera);
			}
		}

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

		/// <summary>
		/// Keep virtual camera children of a state driven camera all
		/// pointing in the same direction when changing between state driven cameras
		/// </summary>
		/// <param name="sourceStateDrivenCamera">The state driven camera that is being transitioned from</param>
		/// <param name="destinationStateDrivenCamera">The state driven camera that is being transitioned to</param>
		void SetCameraAxes(CinemachineStateDrivenCamera sourceStateDrivenCamera,
		                           CinemachineStateDrivenCamera destinationStateDrivenCamera)
		{
			foreach (var camera in sourceStateDrivenCamera.ChildCameras)
			{
				if (sourceStateDrivenCamera.IsLiveChild(camera))
				{
					var cameraX = camera.GetComponent<CinemachineFreeLook>().m_XAxis.Value;
					var cameraY = camera.GetComponent<CinemachineFreeLook>().m_YAxis.Value;
					SetChildCameraAxes(destinationStateDrivenCamera, cameraX, cameraY);
				}
			}
		}

		void SetChildCameraAxes(CinemachineStateDrivenCamera stateDrivenCamera, float xAxis, float yAxis)
		{
			foreach (var childCamera in stateDrivenCamera.ChildCameras)
			{
				childCamera.GetComponent<CinemachineFreeLook>().m_XAxis.Value = xAxis;
				childCamera.GetComponent<CinemachineFreeLook>().m_YAxis.Value = yAxis;
			}
		}

		void SetCrosshairVisible(bool isVisible = true)
		{
			if (m_Crosshair == null)
			{
				return;
			}
			
			m_Crosshair.SetActive(isVisible);
		}

		public void SetStrafeCamera()
		{
			SetCameraAxes(m_ExplorationStateDrivenCamera, m_StrafeStateDrivenCamera);
			SetAnimation(m_StrafeState);
			SetCrosshairVisible();
		}

		public void SetExplorationCamera()
		{
			SetCameraAxes(m_StrafeStateDrivenCamera, m_ExplorationStateDrivenCamera);
			SetAnimation(m_ExplorationState);
			SetCrosshairVisible(false);
		}

		/// <summary>
		/// Sets the <see cref="ThirdPersonBrain"/> and automatically sets up the required fields for the Cinemachine cameras
		/// </summary>
		/// <param name="brainToUse">The third person brain to use</param>
		public void SetThirdPersonBrain(ThirdPersonBrain brainToUse)
		{
			m_ThirdPersonBrain = brainToUse;

			//Automatically handle Cinemachine setup
			if (m_StrafeStateDrivenCamera.m_AnimatedTarget == null)
			{
				m_StrafeStateDrivenCamera.m_AnimatedTarget = m_ThirdPersonBrain.GetComponent<Animator>();
			}

			if (m_ExplorationStateDrivenCamera.m_AnimatedTarget == null)
			{
				m_ExplorationStateDrivenCamera.m_AnimatedTarget = m_ThirdPersonBrain.GetComponent<Animator>();
			}

			var rootSdc = GetComponent<CinemachineStateDrivenCamera>();
			if (rootSdc != null)
			{
				rootSdc.m_LookAt = m_ThirdPersonBrain.transform;
				rootSdc.m_Follow = m_ThirdPersonBrain.transform;
			}
		}
	}
}
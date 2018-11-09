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
		[FormerlySerializedAs("idleCamera")]
		[SerializeField, Tooltip("This is the free look camera that will be able to get recentered")]
		CinemachineFreeLook m_IdleCamera;

		[FormerlySerializedAs("crosshair")]
		[SerializeField, Tooltip("The aiming crosshair that is visible during strafe")]
		GameObject m_Crosshair;

		ThirdPersonBrain m_ThirdPersonBrain;

		Animator m_Animator;


		public void RecenterCamera()
		{
			if (!m_ThirdPersonBrain.thirdPersonInput.hasMovementInput)
			{
				RecenterFreeLookCam(m_IdleCamera);
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
			SetCrosshairVisible();
		}

		public void SetExplorationCamera()
		{
			SetCrosshairVisible(false);
		}

		/// <summary>
		/// Sets the <see cref="ThirdPersonBrain"/> and automatically sets up the required fields for the Cinemachine cameras
		/// </summary>
		/// <param name="brainToUse">The third person brain to use</param>
		public void SetThirdPersonBrain(ThirdPersonBrain brainToUse)
		{
			m_ThirdPersonBrain = brainToUse;

			var rootSdc = GetComponent<CinemachineStateDrivenCamera>();
			if (rootSdc != null)
			{
				rootSdc.m_LookAt = m_ThirdPersonBrain.transform;
				rootSdc.m_Follow = m_ThirdPersonBrain.transform;
				rootSdc.m_AnimatedTarget = m_ThirdPersonBrain.GetComponent<Animator>();
			}
		}
	}
}
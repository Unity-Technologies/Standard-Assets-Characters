using UnityEngine;
using UnityEngine.Serialization;
using Cinemachine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Displays crosshair when strafing, implement recentering on demand
	/// </summary>
	public class ThirdPersonCameraController : MonoBehaviour
	{
		[Tooltip("The aiming crosshair that is visible during strafe")]
		public GameObject m_Crosshair;

		public ThirdPersonBrain m_ThirdPersonBrain;

        CinemachineStateDrivenCamera m_SDC;

        ThirdPersonInput UserInput
        {
            get
            {
                return (m_ThirdPersonBrain == null)
                    ? null : m_ThirdPersonBrain.thirdPersonInput as ThirdPersonInput;
            }
        }

        CinemachineFreeLook LiveFreeLook
        {
            get { return m_SDC == null ? null : m_SDC.LiveChild as CinemachineFreeLook; }
        }

        private void OnEnable()
        {
            var userInput = UserInput;
			if (userInput != null)
			{
				userInput.recentreCamera -= RecenterCamera;
				userInput.recentreCamera += RecenterCamera;
			}
            m_SDC = GetComponent<CinemachineStateDrivenCamera>();
        }

        private void OnDisable()
        {
            var userInput = UserInput;
			if (userInput != null)
			{
				userInput.recentreCamera -= RecenterCamera;
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
            SetCrosshairVisible(m_ThirdPersonBrain.IsStrafing);

            var userInput = UserInput;
            if (userInput != null && (userInput.hasMovementInput || userInput.lookInput != Vector2.zero))
                DisableRecentering(LiveFreeLook);
		}

		void SetCrosshairVisible(bool isVisible = true)
		{
			if (m_Crosshair != null)
    			m_Crosshair.SetActive(isVisible);
		}

        void RecenterCamera()
        {
            var userInput = UserInput;
            if (userInput != null && !userInput.hasMovementInput)
                EnableRecentering(LiveFreeLook);
        }

		void EnableRecentering(CinemachineFreeLook freeLook)
		{
            if (freeLook != null)
            {
			    freeLook.m_RecenterToTargetHeading.m_enabled = true;
			    freeLook.m_YAxisRecentering.m_enabled = true;
		    }
        }

		void DisableRecentering(CinemachineFreeLook freeLook)
		{
            if (freeLook != null)
            {
			    freeLook.m_RecenterToTargetHeading.m_enabled = false;
			    freeLook.m_YAxisRecentering.m_enabled = false;
            }
		}
    }
}
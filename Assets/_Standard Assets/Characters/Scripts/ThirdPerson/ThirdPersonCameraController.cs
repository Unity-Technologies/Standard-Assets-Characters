using System;
using Cinemachine;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Displays crosshair when strafing, implement recentering on demand
	/// </summary>
	public class ThirdPersonCameraController : MonoBehaviour
	{
		[SerializeField, Tooltip("The aiming crosshair that is visible during strafe")]
		GameObject m_Crosshair;

		ThirdPersonBrain m_ThirdPersonBrain;
		CinemachineStateDrivenCamera m_SDC;


        /// <summary>
        /// Gets the <see cref="UserInput"/> for the registered <see cref="ThirdPersonBrain"/>
        /// </summary>
        ThirdPersonInput UserInput
        {
            get { return (m_ThirdPersonBrain != null) ? m_ThirdPersonBrain.thirdPersonInput as ThirdPersonInput : null; }
        }

        /// <summary>
        /// Gets the currently active <see cref="CinemachineFreeLook"/> (if any) on the registered <see cref="CinemachineStateDrivenCamera"/>
        /// </summary>
        CinemachineFreeLook LiveFreeLook
        {
            get { return m_SDC == null ? null : m_SDC.LiveChild as CinemachineFreeLook; }
        }



        /// <summary>
        /// On awake of component
        /// </summary>
        void Awake()
        {
            FindThirdPersonBrain(true);
        }

        /// <summary>
        /// On enable of component
        /// </summary>
        void OnEnable()
        {
        	//register the camera recentering event for input
            var userInput = UserInput;
			if (userInput != null)
			{
				userInput.recentreCamera -= RecenterCamera;
				userInput.recentreCamera += RecenterCamera;
			}

			//register the camera changing event
			if(m_ThirdPersonBrain != null)
			{
				m_ThirdPersonBrain.onCameraChange -= OnCameraChange;
				m_ThirdPersonBrain.onCameraChange += OnCameraChange;
			}
        }

        /// <summary>
        /// On disable of component
        /// </summary>
        void OnDisable()
        {
        	//deregister the camera recentering event
            var userInput = UserInput;
			if (userInput != null)
			{
				userInput.recentreCamera -= RecenterCamera;
			}

			//deregister the camera changing event
			if(m_ThirdPersonBrain != null)
			{
				m_ThirdPersonBrain.onCameraChange -= OnCameraChange;
			}
        }

        /// <summary>
        /// On update of component
        /// </summary>
		void Update()
		{
			if (m_ThirdPersonBrain == null)
			{
				Debug.LogError("No Third Person Brain in the scene", gameObject);
				gameObject.SetActive(false);
				return;
			}

			//don't allow camera recentering if there is any movement or look input from the user
            var userInput = UserInput;
			if ((userInput == null) || userInput.hasMovementInput || (userInput.lookInput != Vector2.zero))
			{
				DisableRecentering(LiveFreeLook);
			}
		}

        /// <summary>
        /// Event registered to handle the Recentering of the CM Freelook camera
        /// </summary>
        void RecenterCamera()
        {
        	//only recenter if there is no user input
            var userInput = UserInput;
            if ((userInput == null) || !userInput.hasMovementInput)
            {
                EnableRecentering(LiveFreeLook);
            }
        }

        /// <summary>
        /// Tells the specified freeLook camera to start recentering
        /// </summary>
        /// <param name="freeLook">CinemachineFreeLook camera that should be recentered</param>
		void EnableRecentering(CinemachineFreeLook freeLook)
		{
			if(freeLook != null)
			{
				freeLook.m_RecenterToTargetHeading.m_enabled = true;
				freeLook.m_YAxisRecentering.m_enabled = true;
			}
		}

        /// <summary>
        /// Tells the specified freeLook camera to stop recentering
        /// </summary>
        /// <param name="freeLook">CinemachineFreeLook camera that should stop being recentered</param>
		void DisableRecentering(CinemachineFreeLook freeLook)
		{
			if(freeLook != null)
			{
				freeLook.m_RecenterToTargetHeading.m_enabled = false;
				freeLook.m_YAxisRecentering.m_enabled = false;
			}
		}

        /// <summary>
        /// Event called whenever the <see cref="ThirdPersonBrain"/> changes cameras
        /// </summary>
		void OnCameraChange()
		{
			//set the crosshairs if we are strafing
			if (m_Crosshair != null)
			{
				m_Crosshair.SetActive(m_ThirdPersonBrain.IsStrafing);
			}
		}

		/// <summary>
		/// Finds the <see cref="ThirdPersonBrain"/> and automatically sets up the required fields for the Cinemachine cameras
		/// </summary>
		void FindThirdPersonBrain(bool autoDisable)
		{
			if(m_ThirdPersonBrain == null)
			{
                ThirdPersonBrain[] thirdPersonBrainObjects = FindObjectsOfType<ThirdPersonBrain>();
                int length = thirdPersonBrainObjects.Length;
                if (length != 1)
                {
		            string errorMessage = "No ThirdPersonBrain in scene! Disabling Camera Controller";
		            if (length > 1)
		            {
		                errorMessage = "Too many ThirdPersonBrains in scene! Disabling Camera Controller";
		            }

		            if (autoDisable)
		            {
		                Debug.LogError(errorMessage);
		                gameObject.SetActive(false);
		            }
		        }
				m_ThirdPersonBrain = thirdPersonBrainObjects[0];
			}

			//auto-set up the necessary state driven camera data
            m_SDC = GetComponent<CinemachineStateDrivenCamera>();
			if (m_SDC != null)
			{
				m_SDC.m_LookAt = m_ThirdPersonBrain.vcamTarget;
				m_SDC.m_Follow = m_ThirdPersonBrain.vcamTarget;
				m_SDC.m_AnimatedTarget = m_ThirdPersonBrain.GetComponent<Animator>();
			}
		}

#if UNITY_EDITOR
        /// <summary>
        /// On reset of component
        /// </summary>
        void Reset()
        {
            //Design pattern for fetching required scene references
            FindThirdPersonBrain(false);
        }

        /// <summary>
        /// On change of component
        /// </summary>
        void OnValidate()
        {
            //Design pattern for fetching required scene references
            FindThirdPersonBrain(false);
        }
#endif		
	}
}
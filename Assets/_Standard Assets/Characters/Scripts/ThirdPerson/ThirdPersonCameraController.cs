using System;
using Cinemachine;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Displays crosshair when strafing, implement recentering on demand
	/// </summary>
	public class ThirdPersonCameraController : MonoBehaviour
	{
		[SerializeField, Tooltip("The aiming crosshair that is visible during strafe")]
		GameObject m_Crosshair;

        // The Third Person Brain object registered to the Controller
		ThirdPersonBrain m_ThirdPersonBrain;

        // The Cinemachine State Driven Camera registered to the Controller
		CinemachineStateDrivenCamera m_SDC;

        // Gets the UserInput for the registered ThirdPersonBrain
        ThirdPersonInput UserInput
        {
            get { return (m_ThirdPersonBrain != null) ? m_ThirdPersonBrain.thirdPersonInput as ThirdPersonInput : null; }
        }

        // Gets the currently active CinemachineFreeLook (if any) on the registered CinemachineStateDrivenCamera
        CinemachineFreeLook LiveFreeLook
        {
            get { return m_SDC == null ? null : m_SDC.LiveChild as CinemachineFreeLook; }
        }

        // On awake of component
        void Awake()
        {
	        SetupMainCamera();
            FindThirdPersonBrain(true);
        }

#if UNITY_EDITOR
        /// On reset of component
        void Reset()
        {
            //Design pattern for fetching required scene references
            FindThirdPersonBrain(false);
	        SetupMainCamera();
        }

        /// On change of component
        void OnValidate()
        {
            //Design pattern for fetching required scene references
            FindThirdPersonBrain(false);
	        SetupMainCamera();
        }
#endif      

        // On enable of component
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

        // On disable of component
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

        // On update of component
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

        // Event registered to handle the Recentering of the CM Freelook camera
        void RecenterCamera()
        {
        	//only recenter if there is no user input
            var userInput = UserInput;
            if ((userInput == null) || !userInput.hasMovementInput)
            {
                EnableRecentering(LiveFreeLook);
            }
        }

        // Tells the specified freeLook camera to start recentering
        //      freeLook: CinemachineFreeLook camera that should be recentered
		void EnableRecentering(CinemachineFreeLook freeLook)
		{
			if(freeLook != null)
			{
				freeLook.m_RecenterToTargetHeading.m_enabled = true;
				freeLook.m_YAxisRecentering.m_enabled = true;
			}
		}

        // Tells the specified freeLook camera to stop recentering
        //      freeLook: CinemachineFreeLook camera that should be stop being recentered
		void DisableRecentering(CinemachineFreeLook freeLook)
		{
			if(freeLook != null)
			{
				freeLook.m_RecenterToTargetHeading.m_enabled = false;
				freeLook.m_YAxisRecentering.m_enabled = false;
			}
		}

        // Event called whenever the ThirdPersonBrain changes cameras
		void OnCameraChange()
		{
			//set the crosshairs if we are strafing
			if (m_Crosshair != null)
			{
				m_Crosshair.SetActive(m_ThirdPersonBrain.IsStrafing);
			}
		}
		
		// Ensures that the main camera has a CinemachineBrain
		void SetupMainCamera()
		{
			var mainCamera = Camera.main;
			if (mainCamera != null && mainCamera.GetComponent<CinemachineBrain>() == null)
			{
				mainCamera.gameObject.AddComponent<CinemachineBrain>();
			}
		}

		// Finds the ThirdPersonBrain and automatically sets up the required fields for the Cinemachine cameras
		void FindThirdPersonBrain(bool autoDisable)
		{
			if(m_ThirdPersonBrain == null)
			{
                ThirdPersonBrain[] thirdPersonBrainObjects = FindObjectsOfType<ThirdPersonBrain>();
                int length = thirdPersonBrainObjects.Length;
				bool found = true;
                if (length != 1)
                {
		            string errorMessage = "No ThirdPersonBrain in scene! Disabling Camera Controller";
		            if (length > 1)
		            {
		                errorMessage = "Too many ThirdPersonBrains in scene! Disabling Camera Controller";
		            }
		            else // none found
		            {
			            found = false;
		            }

		            if (autoDisable)
		            {
		                Debug.LogError(errorMessage);
#if UNITY_EDITOR
			            EditorUtility.DisplayDialog("Error detecting ThirdPersonBrain", errorMessage, "Ok");
#endif
		                gameObject.SetActive(false);
		            }
		        }

				if (found)
				{
					m_ThirdPersonBrain = thirdPersonBrainObjects[0];
				}
				else
				{
					return;
				}
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
	}
}
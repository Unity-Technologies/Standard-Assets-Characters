using Cinemachine;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StandardAssets.Characters.FirstPerson
{
    /// <summary>
    /// Manages first person cameras 
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FirstPersonCameraController : MonoBehaviour
    {
        /// <summary>
        /// The names of the states in the camera animator
        /// </summary>
        const string k_CrouchAnimationStateName = "Crouch";
        const string k_SprintAnimationStateName = "Sprint";
        const string k_WalkAnimationStateName = "Walk";

        // Unity Animator with blank animation states for Cinemachine State Driven Cameras
        Animator m_Animator;

        // Brain 
        FirstPersonBrain m_FirstPersonBrain;

        /// <summary>
        /// Sets the animation to the defined state
        /// </summary>
        /// <param name="state">the name of the animation state</param>
        /// <param name="layer">the animation layer to use to play animation</param>
        public void SetAnimation(string state, int layer = 0)
        {
            if (m_Animator == null)
            {
                m_Animator = GetComponent<Animator>();
            }

            m_Animator.Play(state, layer);
        }

        // On awake of component
        void Awake()
        {
            FindFirstPersonBrain(true);
            SetupMainCamera();
        }

        // Subscribes to movement state changes
        void OnEnable()
        {
            if (m_FirstPersonBrain == null)
            {
                return;
            }

            m_FirstPersonBrain.startWalking += OnStartedWalking;
            m_FirstPersonBrain.startCrouching += OnStartCrouching;
            m_FirstPersonBrain.startSprinting += OnStartSprinting;
        }
        
        // Unsubscribes from movement state changes
        void OnDisable()
        {
            if (m_FirstPersonBrain == null)
            {
                return;
            }

            m_FirstPersonBrain.startWalking -= OnStartedWalking;
            m_FirstPersonBrain.startCrouching -= OnStartCrouching;
            m_FirstPersonBrain.startSprinting -= OnStartSprinting;
        }

#if UNITY_EDITOR
        /// On reset of component
        void Reset()
        {
            //Design pattern for fetching required scene references
            FindFirstPersonBrain(false);
            SetupMainCamera();
        }

        /// On change of component
        void OnValidate()
        {
            //Design pattern for fetching required scene references
            FindFirstPersonBrain(false);
            SetupMainCamera();
        }
#endif

        // Ensures that the main camera has a CinemachineBrain
        void SetupMainCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<CinemachineBrain>() == null)
            {
                mainCamera.gameObject.AddComponent<CinemachineBrain>();
            }
        }
        
        // Finds the FirstPersonBrain and automatically sets up the required fields for the Cinemachine cameras
        void FindFirstPersonBrain(bool autoDisable)
        {
            if (m_FirstPersonBrain == null)
            {
                FirstPersonBrain[] firstPersonBrains = FindObjectsOfType<FirstPersonBrain>();
                int length = firstPersonBrains.Length;
                bool found = true;
                if (length != 1)
                {
                    string errorMessage = "No FirstPersonBrain in scene! Disabling Camera Controller";
                    if (length > 1)
                    {
                        errorMessage = "Too many FirstPersonBrain in scene! Disabling Camera Controller";
                    }
                    else // none found
                    {
                        found = false;
                    }

                    if (autoDisable)
                    {
                        Debug.LogError(errorMessage);
#if UNITY_EDITOR
                        EditorUtility.DisplayDialog("Error detecting FirstPersonBrain", errorMessage, "Ok");
#endif
                        gameObject.SetActive(false);
                    }
                }

                if (found)
                {
                    m_FirstPersonBrain = firstPersonBrains[0];
                }
                else
                {
                    return;
                }

                var rootSdc = GetComponent<CinemachineStateDrivenCamera>();
                if (rootSdc != null)
                {
                    rootSdc.m_Follow = m_FirstPersonBrain.transform;
                }
            }
        }
        
        // Sets camera animation to sprint
        void OnStartSprinting()
        {
            SetAnimation(k_SprintAnimationStateName);
        }

        // Sets camera animation to crouch
        void OnStartCrouching()
        {
            SetAnimation(k_CrouchAnimationStateName);
        }

        // Sets camera animation to walk
        void OnStartedWalking()
        {
            SetAnimation(k_WalkAnimationStateName);
        }
    }
}

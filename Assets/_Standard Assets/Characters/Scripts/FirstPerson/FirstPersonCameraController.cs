using Cinemachine;
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
    /// <summary>
    /// Manages first person cameras 
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FirstPersonCameraController : MonoBehaviour
    {
        private Animator animator;
		
        /// <summary>
        /// Sets the animation to the defined state
        /// </summary>
        /// <param name="state">the name of the animation state</param>
        public void SetAnimation(string state, int layer = 0)
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            animator.Play(state,layer);
        }
        
        /// <summary>
        /// Sends through the <see cref="FirstPersonBrain"/> and allows Follow field of <see cref="CinemachineStateDrivenCamera"/> to be set correctly
        /// </summary>
        /// <param name="brainToUse">The FirstPersonBrain component which has a transform for the <see cref="CinemachineStateDrivenCamera"/> to follow</param>
        public void SetupBrain(FirstPersonBrain brainToUse)
        {
            CinemachineStateDrivenCamera rootSdc = GetComponent<CinemachineStateDrivenCamera>();
            if (rootSdc != null)
            {
                rootSdc.m_Follow = brainToUse.transform;
            }
        }
    }
}
using Cinemachine;
using StandardAssets.Characters.Common;

namespace StandardAssets.Characters.FirstPerson
{
    /// <summary>
    /// Implementation of <see cref="CameraController"/> to manage first person cameras 
    /// </summary>
    public class FirstPersonCameraController : CameraController
    {
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
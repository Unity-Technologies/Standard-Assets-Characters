using Cinemachine;
using StandardAssets.Characters.Common;

namespace StandardAssets.Characters.FirstPerson
{
    /// <summary>
    /// Implementation of <see cref="CameraAnimationManager"/> to manage first person cameras 
    /// </summary>
    public class FirstPersonCameraAnimationManager : CameraAnimationManager
    {
        /// <summary>
        /// Sends through the <see cref="FirstPersonBrain"/> and allows the SDC to setup correct follow
        /// </summary>
        /// <param name="brainToUse"></param>
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
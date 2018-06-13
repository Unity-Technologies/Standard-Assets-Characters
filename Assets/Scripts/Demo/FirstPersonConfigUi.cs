using StandardAssets.Characters.FirstPerson;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    /// <summary>
    /// Exposes movement properties from the first person controller for a UI
    /// </summary>
    public class FirstPersonConfigUi : MonoBehaviour
    {
        public Text currentMovementSpeedText;
        public FirstPersonController fpsController;

        private void Update()
        {
            UpdateCurrentMovementSpeedProperty();
        }
        
        /// <summary>
        /// Updates UI Label with current state max speed
        /// </summary>
        void UpdateCurrentMovementSpeedProperty()
        {
            currentMovementSpeedText.text = fpsController.currentMovementProperties.maximumSpeed.ToString();
        }
    }
}
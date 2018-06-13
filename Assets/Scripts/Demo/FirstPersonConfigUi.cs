using StandardAssets.Characters.CharacterInput;
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

        

        public Slider speedSlider;

        private float currentSpeed;
        

        private float minSpeed = 1;
        private float maxSpeed = 15;
        private float speedRange = 14;

       
        private void Update()
        {
            UpdateSpeedSlider();
        }
        
        /// <summary>
        /// Updates UI Label with current state max speed
        /// </summary>
        void UpdateSpeedSlider()
        {
            currentSpeed = fpsController.currentMovementProperties.maximumSpeed;
            currentMovementSpeedText.text = currentSpeed.ToString("0.0");
            speedSlider.value = (currentSpeed - minSpeed) / speedRange;
            
        }

        public void UpdateSpeedForCurrentState(Slider slider)
        {
            float speed = (slider.value*speedRange)+minSpeed;
            fpsController.currentMovementProperties.maximumSpeed = speed;
        }
    }
}
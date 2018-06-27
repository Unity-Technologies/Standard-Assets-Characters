using StandardAssets.Characters.ThirdPerson;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class ThirdPersonMovementSpeedConfigUi: MonoBehaviour
    {
        [SerializeField]
        protected Text currentMovementSpeedText;
        [SerializeField]
        protected PhysicsThirdPersonMotor thirdPersonPhysicsMotor;
        [SerializeField]
        protected Slider speedSlider;

        private float currentSpeed;
        private float minSpeed = 1;
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
           currentSpeed = thirdPersonPhysicsMotor.currentSpeedForUi;
           currentMovementSpeedText.text = currentSpeed.ToString("0.0");
           speedSlider.value = (currentSpeed - minSpeed) / speedRange;
        }

        public void UpdateSpeedForCurrentState(Slider slider)
        {
            float speed = (slider.value*speedRange)+minSpeed;
           thirdPersonPhysicsMotor.currentSpeedForUi = speed;
        }
    }
}
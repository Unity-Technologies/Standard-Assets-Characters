using StandardAssets.Characters.ThirdPerson;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class ThirdPersonMovementSpeedConfigUi: MonoBehaviour
    {
        public Text currentMovementSpeedText;
        public PhysicsThirdPersonMotor thirdPersonPhysicsMotor;

        

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
          //  Debug.Log(thirdPersonPhysicsMotor.currentSpeedForUIget);
            
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
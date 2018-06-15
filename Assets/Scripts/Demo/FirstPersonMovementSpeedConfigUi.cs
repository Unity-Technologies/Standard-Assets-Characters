using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.FirstPerson;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    /// <summary>
    /// Exposes movement properties from the first person controller for a UI
    /// </summary>
    public class FirstPersonMovementSpeedConfigUi : MonoBehaviour
    {
        public Text currentMovementSpeedText;
        public FirstPersonController fpsController;

        

        private FirstPersonMovementModification[] modifiers;

       // public Slider speedSlider;

        private float currentSpeed;
        

        private float minSpeed = 1;
        private float maxSpeed = 15;
        private float speedRange = 14;

        public Slider walkSlider;
        public Slider sprintSlider;
        public Slider crouchSlider;
        public Slider proneSlider;

        public Text walkSpeedTextValue;
        public Text sprintSpeedTextValue;
        public Text crouchSpeedTextValue;
        public Text proneSpeedTextValue;

        private float maxWalkSpeedValue;
        private float maxSprintSpeedValue;
        private float maxCrouchSpeedValue;
        private float maxProneSpeedValue;
        

        void Start()
        {
            modifiers = fpsController.exposedMovementModifiers;
            
            
            maxWalkSpeedValue= fpsController.currentMovementProperties.maximumSpeed;
            maxCrouchSpeedValue = modifiers[0].getMovementProperty().maximumSpeed;
            maxProneSpeedValue= modifiers[1].getMovementProperty().maximumSpeed;
            maxSprintSpeedValue = modifiers[2].getMovementProperty().maximumSpeed;
            
            SetInitialSliderValues(walkSlider,walkSpeedTextValue,maxWalkSpeedValue);
            SetInitialSliderValues(crouchSlider,crouchSpeedTextValue,maxCrouchSpeedValue);
            SetInitialSliderValues(proneSlider,proneSpeedTextValue,maxProneSpeedValue);
            SetInitialSliderValues(sprintSlider,sprintSpeedTextValue,maxSprintSpeedValue);

        }

        void SetInitialSliderValues(Slider slider, Text valueLabel, float value)
        {
            valueLabel.text = value.ToString();
            SetSliderPosition(slider,value);
        }

        void SetSliderPosition(Slider slider, float value)
        {
            slider.value = (value - minSpeed) / speedRange;
        }
       
        
        /// <summary>
        /// Updates UI Label with current state max speed
        /// </summary>
        void UpdateSpeedSlider()
        {
            currentSpeed = fpsController.currentMovementProperties.maximumSpeed;
            currentMovementSpeedText.text = currentSpeed.ToString("0.0");
            //speedSlider.value = (currentSpeed - minSpeed) / speedRange;
            
        }

        public void UpdateSpeedForCurrentState(Slider slider)
        {
            float speed = (slider.value*speedRange)+minSpeed;
            fpsController.currentMovementProperties.maximumSpeed = speed;
        }

        public void SetWalkSpeed(Slider slider)
        {
            float speed = (slider.value*speedRange)+minSpeed;
            fpsController.currentMovementProperties.maximumSpeed = speed;
            walkSpeedTextValue.text = speed.ToString("0");
        }

        public void SetSprintSpeed(Slider slider)
        {
            float speed = (slider.value*speedRange)+minSpeed;
            modifiers[2].getMovementProperty().maximumSpeed = speed;
            sprintSpeedTextValue.text = speed.ToString("0");
        }

        public void SetCrouchSpeed(Slider slider)
        {
            float speed = (slider.value*speedRange)+minSpeed;
            modifiers[0].getMovementProperty().maximumSpeed = speed;
            crouchSpeedTextValue.text = speed.ToString("0");
        }
        
        public void SetProneSpeed(Slider slider)
        {
            float speed = (slider.value*speedRange)+minSpeed;
            modifiers[1].getMovementProperty().maximumSpeed = speed;
            proneSpeedTextValue.text = speed.ToString("0");
        }
    }
}
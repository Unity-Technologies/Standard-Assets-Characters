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
        [SerializeField]
        protected FirstPersonController fpsController;
        
        private FirstPersonMovementModification[] modifiers;
        
        private float minMovementSpeed = 1;
        private float movementSpeedRange = 14;
        
        /// <summary>
        /// UI Elements 
        /// </summary>
        public Slider startingSpeedSlider,sprintSlider,
                crouchSlider,proneSlider;
    
        public Text startingSpeedTextValue, sprintSpeedTextValue,
            crouchSpeedTextValue,proneSpeedTextValue;
   
        private float startingMaxSpeedValue = 0, maxSprintSpeedValue = 0,
            maxCrouchSpeedValue = 0, maxProneSpeedValue = 0;
 
        private Text[] sliderTextValues;
        private float[] sliderValues;
        private Slider[] sliders;

        void Start()
        {
           // modifiers = fpsController.exposedMovementModifiers;

            sliderTextValues = new Text[] {sprintSpeedTextValue, crouchSpeedTextValue, proneSpeedTextValue};
            sliderValues = new float[] {maxSprintSpeedValue, maxCrouchSpeedValue, maxProneSpeedValue};
            sliders = new Slider[] {sprintSlider, crouchSlider, proneSlider};
            
            //Walk is sperate as it is the starting values
            startingMaxSpeedValue= fpsController.currentMovementProperties.maximumSpeed;
            SetInitialSliderValues(startingSpeedSlider,startingSpeedTextValue,startingMaxSpeedValue);

            for (int i = 0; i < sliders.Length; i++)
            {
                sliderValues[i] = modifiers[i].GetMovementProperty().maximumSpeed;
                SetInitialSliderValues(sliders[i],sliderTextValues[i],sliderValues[i]);
            }
           
        }

        void SetInitialSliderValues(Slider slider, Text valueLabel, float value)
        {
            valueLabel.text = value.ToString();
            slider.value = (value - minMovementSpeed) / movementSpeedRange;
           
        }
        
        public void SetWalkSpeed(Slider slider)
        {
            float speed = ScaleSpeed(slider.value);
            fpsController.currentMovementProperties.maximumSpeed = speed;
            startingSpeedTextValue.text = speed.ToString("0");
        }

        public void SetSprintSpeed(Slider slider)
        {  
            UpdateSlider(0, slider);
        }

        public void SetCrouchSpeed(Slider slider)
        {   
            UpdateSlider(1, slider);
        }
        
        public void SetProneSpeed(Slider slider)
        {
            UpdateSlider(2, slider);
        }

        float ScaleSpeed(float value)
        {
         return (value*movementSpeedRange)+minMovementSpeed;   
        }

        void UpdateSlider(int index, Slider slider)
        {
            float speed = ScaleSpeed(slider.value);
            modifiers[index].GetMovementProperty().maximumSpeed = speed;
            sliderTextValues[index].text = speed.ToString("0");
        }
        
    }
}
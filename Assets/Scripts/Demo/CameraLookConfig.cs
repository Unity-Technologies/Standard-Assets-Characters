using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class CameraLookConfig:MonoBehaviour
    {
        /// <summary>
        /// Camera peramiters 
        /// </summary>
        public bool invertXAxis{ get; set; }
        public bool invertYAxis{ get; set; }

        private float xAxisMaxSpeed;
        private float yAxisMaxSpeed;
        
        /// <summary>
        /// Cinemachine FreeLook Cameras
        /// </summary>
        public CinemachineFreeLook[] cameras;
        
        
        /// <summary>
        /// UI Label Elements 
        /// </summary>
        public Text verticalSliderValueText;
        public Text horizontalSliderValueText;

        /// <summary>
        /// UI Interactive elements 
        /// </summary>
        public Toggle xAxisToggle;
        public Toggle yAxisToggle;

        public Slider horizontalSlider;
        public Slider verticalSlider;
       
        void Start()
        {
            //SetInitalValues for cameraInversion toggles
            xAxisToggle.isOn = cameras[0].m_XAxis.m_InvertInput;
            yAxisToggle.isOn = cameras[0].m_YAxis.m_InvertInput;
            
           
            xAxisMaxSpeed = (cameras[0].m_XAxis.m_MaxSpeed-100)/300;
            SetMaxSpeedSliderInitialValues(horizontalSlider, xAxisMaxSpeed, horizontalSliderValueText, ((xAxisMaxSpeed*300)+100));
            
           
            yAxisMaxSpeed = (cameras[0].m_YAxis.m_MaxSpeed-1)/5;
            SetMaxSpeedSliderInitialValues(verticalSlider, yAxisMaxSpeed, verticalSliderValueText,((yAxisMaxSpeed*5)+1));
            
            
        }

        void SetMaxSpeedSliderInitialValues(Slider slider, float value, Text sliderText, float scaledValue)
        {
            slider.value = value;
            sliderText.text= scaledValue.ToString("0");
        }
        
        /// <summary>
        /// Sets the vertical sensitivity
        /// </summary>
        public void SetVerticalSliderValue(Slider slider)
        {
            yAxisMaxSpeed = ((slider.value * 5) + 1);
            verticalSliderValueText.text = yAxisMaxSpeed.ToString("0");
            foreach (var camera in cameras)
            {
                camera.m_YAxis.m_MaxSpeed = yAxisMaxSpeed;
            }
        }
        
        /// <summary>
        /// Sets the horizontal sensisitivity
        /// </summary>
        public void SetHorizontalSliderValue(Slider slider)
        {
            xAxisMaxSpeed = ((slider.value * 300) + 100);
            horizontalSliderValueText.text = xAxisMaxSpeed.ToString("0");
            foreach (var camera in cameras)
            {
                camera.m_XAxis.m_MaxSpeed = xAxisMaxSpeed;
            }
        }

        public void ToggleXAxis(Toggle toggle)
        {
            invertXAxis = toggle.isOn;
            foreach (var camera in cameras)
            {
                camera.m_XAxis.m_InvertInput = invertXAxis;
            }
        }
        
        public void ToggleYAxis(Toggle toggle)
        {
            invertYAxis = toggle.isOn;
            foreach (var camera in cameras)
            {
                camera.m_YAxis.m_InvertInput = invertYAxis;
            }
        }

        
    }
}
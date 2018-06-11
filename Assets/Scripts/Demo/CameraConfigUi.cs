using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.UI;

namespace Demo
{
	public abstract class CameraConfigUi<T> : MonoBehaviour where T : CinemachineVirtualCameraBase
	{
		/// <summary>
		/// Camera parameters 
		/// </summary>
		public bool invertXAxis { get; set; }
		public bool invertYAxis { get; set; }

		/// <summary>
		/// Cinemachine FreeLook Cameras
		/// </summary>
		public T[] cameras;

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

		protected float m_XAxisMaxSpeed;
		protected float m_YAxisMaxSpeed;

		public float minXAxisSpeed;
		public float maxXAxisSpeed;
		public float minYAxisSpeed;
		public float maxYAxisSpeed;


		public Text activeInputDevices;


		protected virtual void SetMaxSpeedSliderInitialValues(Slider slider, float value, Text sliderText, float scaledValue)
		{
			slider.value = value;
			sliderText.text = scaledValue.ToString("0");
		}

		/// <summary>
		/// Sets the vertical sensitivity
		/// </summary>
		public void SetVerticalSliderValue(Slider slider)
		{
			m_YAxisMaxSpeed = ((slider.value * (maxYAxisSpeed-minYAxisSpeed)) + minYAxisSpeed);
			verticalSliderValueText.text = m_YAxisMaxSpeed.ToString("0");
			foreach (var camera in cameras)
			{
				SetYAxisMaxSpeed(camera, m_YAxisMaxSpeed);
			}
		}

		

		/// <summary>
		/// Sets the horizontal sensisitivity
		/// </summary>
		public void SetHorizontalSliderValue(Slider slider)
		{
			m_XAxisMaxSpeed = ((slider.value * (maxXAxisSpeed-minXAxisSpeed)) + minXAxisSpeed);
			horizontalSliderValueText.text = m_XAxisMaxSpeed.ToString("0");
			foreach (var camera in cameras)
			{
				SetXAxisMaxSpeed(camera, m_XAxisMaxSpeed);
			}
		}

		public void ToggleXAxis(Toggle toggle)
		{
			invertXAxis = toggle.isOn;
			foreach (var camera in cameras)
			{
				InvertXAxis(camera, invertXAxis);
			}
		}

		public void ToggleYAxis(Toggle toggle)
		{
			invertYAxis = toggle.isOn;
			foreach (var camera in cameras)
			{
				InvertYAxis(camera, invertYAxis);
			}
		}
		
		/// <summary>
		/// Get the active input devices 
		/// </summary>
		protected virtual void UpdateActiveInputDevices()
		{
			string devices = ""; 
			foreach (var device in InputSystem.devices)
			{
				devices += ">"+device.ToString()+"\n";
				
			}

			activeInputDevices.text = devices;
		}


		void Update()
		{
			UpdateActiveInputDevices();
			
		}

		protected abstract void SetYAxisMaxSpeed(T camera, float newMaxYAxisMaxSpeed);
		protected abstract void SetXAxisMaxSpeed(T camera, float newMaxXAxisMaxSpeed);
		protected abstract void InvertXAxis(T camera, bool invert);
		protected abstract void InvertYAxis(T camera, bool invert);
	}
}
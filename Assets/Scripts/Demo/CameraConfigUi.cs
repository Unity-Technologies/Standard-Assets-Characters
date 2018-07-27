using Cinemachine;
using UnityEngine;
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
		[SerializeField]
		protected T[] cameras;

		/// <summary>
		/// Gamepad camera config UI elements
		/// </summary>
		[SerializeField]
		protected Text verticalSliderGamepadValueText;
		[SerializeField]
		protected Text horizontalSliderGamepadValueText;
		
		[SerializeField]
		protected Toggle xAxisToggleGamepad;
		[SerializeField]
		protected Toggle yAxisToggleGamepad;
		
		[SerializeField]
		protected Slider horizontalSliderGamepad;
		[SerializeField]
		protected Slider verticalSliderGamepad;
		
		[SerializeField]
		protected float m_XAxisMaxSpeed;
		[SerializeField]
		protected float m_YAxisMaxSpeed;
		
		/// <summary>
		/// Mouse look camera config UI elements
		/// </summary>
		[SerializeField]
		protected Text xAxisSliderValueTextMouse;
		[SerializeField]
		protected Text yAxisSliderValueTextMouse;

		[SerializeField]
		protected Slider horizontalSliderMouse;
		[SerializeField]
		protected Slider verticalSliderMouse;

		[SerializeField]
		protected float xAxisMaxSpeedMouse;
		[SerializeField]
		protected float yAxisMaxSpeedMouse;
		
	
		protected virtual void SetMaxSpeedSliderInitialValues(Slider slider, float value, Text sliderText, float scaledValue)
		{
			slider.value = value;
			sliderText.text = scaledValue.ToString("0");
		}

		/// <summary>
		/// Sets the vertical sensitivity for gamepad look 
		/// </summary>
		public void SetVerticalSliderValue(Slider slider)
		{
			m_YAxisMaxSpeed = ((slider.value * 400) + 100);
			verticalSliderGamepadValueText.text = (m_YAxisMaxSpeed).ToString("0");
			foreach (var camera in cameras)
			{
				SetYAxisMaxSpeed(camera, m_YAxisMaxSpeed);
			}
		}

		/// <summary>
		/// Sets the horizontal sensisitivity for gamepad look
		/// </summary>
		public void SetHorizontalSliderValue(Slider slider)
		{
			m_XAxisMaxSpeed = ((slider.value * 400) + 100);
			horizontalSliderGamepadValueText.text = (m_XAxisMaxSpeed).ToString("0");
			foreach (var camera in cameras)
			{
				SetXAxisMaxSpeed(camera, m_XAxisMaxSpeed);
			}
		}

		/// <summary>
		/// Toggle XAxis on right stick gamepad look
		/// </summary>
		public void ToggleXAxis(Toggle toggle)
		{
			invertXAxis = toggle.isOn;
			foreach (var camera in cameras)
			{
				InvertXAxis(camera, invertXAxis);
			}
		}

		/// <summary>
		/// Toggle YAxis on right stick gamepad look
		/// </summary>
		public void ToggleYAxis(Toggle toggle)
		{
			invertYAxis = toggle.isOn;
			foreach (var camera in cameras)
			{
				InvertYAxis(camera, invertYAxis);
			}
		}

		float GetMouseSensitivityValue(float current, Slider slider)
		{
			var newSensitivity = slider.value  * 20;
			
			if (current < 0)
			{
				newSensitivity *= -1;
			}

			return newSensitivity;
		}

		void SetMouseSensitivytSliderText(Text text, float value)
		{
			text.text = Mathf.Abs(value).ToString("0");
		}
		

		protected abstract void SetYAxisMaxSpeed(T camera, float newMaxYAxisMaxSpeed);
		protected abstract void SetXAxisMaxSpeed(T camera, float newMaxXAxisMaxSpeed);
		protected abstract void InvertXAxis(T camera, bool invert);
		protected abstract void InvertYAxis(T camera, bool invert);
	}
}
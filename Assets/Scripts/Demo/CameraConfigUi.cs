using System.Text;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.FirstPerson;
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

		[SerializeField]
		protected FirstPersonMouseLookPOVCamera povCamAdjustables; //Mouse sensitivity(for POV cameras)

		/// <summary>
		/// Cinemachine FreeLook Cameras
		/// </summary>
		[SerializeField]
		protected T[] cameras;

		/// <summary>
		/// UI Label Elements 
		/// </summary>
		[SerializeField]
		protected Text verticalSliderValueText;

		[SerializeField]
		protected Text horizontalSliderValueText;

		/// <summary>
		/// UI Interactive elements 
		/// </summary>
		[SerializeField]
		protected Toggle xAxisToggle;

		[SerializeField]
		protected Toggle yAxisToggle;

		[SerializeField]
		protected Toggle xAxisToggleMouse;
		[SerializeField]
		protected Toggle yAxisToggleMouse;

		[SerializeField]
		protected Slider horizontalSliderMouse;
		[SerializeField]
		protected Slider verticalSliderMouse;
		[SerializeField]
		protected Text xAxisSliderValueTextMouse;
		[SerializeField]
		protected Text yAxisSliderValueTextMouse;

		[SerializeField]
		protected Slider horizontalSlider;
		[SerializeField]
		protected Slider verticalSlider;


		[SerializeField]
		protected float minXAxisSpeed;
		[SerializeField]
		protected float maxXAxisSpeed;
		[SerializeField]
		protected float minYAxisSpeed;
		[SerializeField]
		protected float maxYAxisSpeed;

		[SerializeField]
		protected Text activeInputDevices;

		protected float m_XAxisMaxSpeed;
		protected float m_YAxisMaxSpeed;
		protected float xAxisMaxSpeedMouse;
		protected float yAxisMaxSpeedMouse;

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
			m_YAxisMaxSpeed = ((slider.value * 400) + 100);
			verticalSliderValueText.text = (m_YAxisMaxSpeed).ToString("0");
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
			m_XAxisMaxSpeed = ((slider.value * 400) + 100);
			horizontalSliderValueText.text = (m_XAxisMaxSpeed).ToString("0");
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

		public void ToggleXAxisMouse(Toggle toggle)
		{
			if (povCamAdjustables == null)
			{
				return;
			}
		
			povCamAdjustables.InvertXAxis();
		}

		public void ToggleYAxisMouse(Toggle toggle)
		{
			if (povCamAdjustables == null)
			{
				return;
			}
			povCamAdjustables.InvertYAxis();
		}

		public void SetMouseXSpeed(Slider slider)
		{
			if (povCamAdjustables == null)
			{
				return;
			}
			var newSensitivity = GetMouseSensitivityValue(povCamAdjustables.GetSensitivity().x, slider);
			povCamAdjustables.SetSensitivity(newSensitivity,0);
			SetMouseSensitivytSliderText(xAxisSliderValueTextMouse, newSensitivity);
		}
		
		public void SetMouseYSpeed(Slider slider)
		{
			if (povCamAdjustables == null)
			{
				return;
			}

			var newSensitivity = GetMouseSensitivityValue(povCamAdjustables.GetSensitivity().y, slider);
			povCamAdjustables.SetSensitivity(0,newSensitivity);
			SetMouseSensitivytSliderText(yAxisSliderValueTextMouse, newSensitivity);
		}

		float GetMouseSensitivityValue(float current, Slider slider)
		{
			var newSensitivity = slider.value  * 10;
			
			if (current < 0)
			{
				newSensitivity *= -1;
			}

			return newSensitivity;
		}

		void SetMouseSensitivytSliderText(Text text, float value)
		{
			text.text = Mathf.Abs(value * 10).ToString("0");
		}
		
		/// <summary>
		/// Get the active input devices 
		/// </summary>
		protected virtual void UpdateActiveInputDevices()
		{
			var devices = new StringBuilder();
			//foreach (var device in InputSystem.devices)
			//{
				//devices.AppendFormat(">{0}\n", device);
				
			//}
			activeInputDevices.text = devices.ToString();
		}


		void Update()
		{
			//UpdateActiveInputDevices();
			
		}

		protected abstract void SetYAxisMaxSpeed(T camera, float newMaxYAxisMaxSpeed);
		protected abstract void SetXAxisMaxSpeed(T camera, float newMaxXAxisMaxSpeed);
		protected abstract void InvertXAxis(T camera, bool invert);
		protected abstract void InvertYAxis(T camera, bool invert);
	}
}
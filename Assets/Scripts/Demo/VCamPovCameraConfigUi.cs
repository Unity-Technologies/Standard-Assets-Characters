using Cinemachine;
using UnityEngine;

namespace Demo
{
	public class VCamPovCameraConfigUi : CameraConfigUi<CinemachineVirtualCamera>
	{
		protected void Start()
		{
			float minSpeedCinemachineGamepad = 100;
			float speedRange = 400;
			
			//SetInitalValues for cameraInversion toggles
			xAxisToggleGamepad.isOn = cameras[0].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InvertInput;
			yAxisToggleGamepad.isOn = cameras[0].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InvertInput;

			m_XAxisMaxSpeed = (cameras[0].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed - minSpeedCinemachineGamepad) / speedRange;
			
			SetMaxSpeedSliderInitialValues(horizontalSliderGamepad, m_XAxisMaxSpeed, horizontalSliderGamepadValueText,
				((m_XAxisMaxSpeed * speedRange)+minSpeedCinemachineGamepad));

			m_YAxisMaxSpeed = 0.3f;
			//m_YAxisMaxSpeed	= (cameras[0].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed - minSpeed) / speedRange;
			SetMaxSpeedSliderInitialValues(verticalSliderGamepad, m_YAxisMaxSpeed, verticalSliderGamepadValueText,
				((m_YAxisMaxSpeed * speedRange)+minSpeedCinemachineGamepad));

			if (povCamAdjustables != null)
			{
				xAxisMaxSpeedMouse = Mathf.Abs(povCamAdjustables.GetSensitivity().x );
				yAxisMaxSpeedMouse = Mathf.Abs(povCamAdjustables.GetSensitivity().y );
			}

			
			SetMaxSpeedSliderInitialValues(horizontalSliderMouse,xAxisMaxSpeedMouse/20,xAxisSliderValueTextMouse,xAxisMaxSpeedMouse);
			SetMaxSpeedSliderInitialValues(verticalSliderMouse,yAxisMaxSpeedMouse/20,yAxisSliderValueTextMouse,yAxisMaxSpeedMouse);
			

		}

		protected override void SetYAxisMaxSpeed(CinemachineVirtualCamera camera, float newMaxYAxisMaxSpeed)
		{
			camera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = newMaxYAxisMaxSpeed;
		}

		protected override void SetXAxisMaxSpeed(CinemachineVirtualCamera camera, float newMaxXAxisMaxSpeed)
		{
			camera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = newMaxXAxisMaxSpeed;
		}

		protected override void InvertXAxis(CinemachineVirtualCamera camera, bool invert)
		{
			camera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InvertInput = invert;
		}

		protected override void InvertYAxis(CinemachineVirtualCamera camera, bool invert)
		{
			camera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InvertInput = invert;
		}
	}
}
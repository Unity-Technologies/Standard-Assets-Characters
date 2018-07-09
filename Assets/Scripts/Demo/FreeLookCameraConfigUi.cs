using Cinemachine;
using UnityEngine.UI;

namespace Demo
{
	public class FreeLookCameraConfigUi : CameraConfigUi<CinemachineFreeLook>
	{
		protected void Start()
		{
			//SetInitalValues for cameraInversion toggles
			xAxisToggleGamepad.isOn = cameras[0].m_XAxis.m_InvertInput;
			yAxisToggleGamepad.isOn = cameras[0].m_YAxis.m_InvertInput;

			m_XAxisMaxSpeed = (cameras[0].m_XAxis.m_MaxSpeed - 100) / 300;
			SetMaxSpeedSliderInitialValues(horizontalSliderGamepad, m_XAxisMaxSpeed, horizontalSliderGamepadValueText,
			                               ((m_XAxisMaxSpeed * 300) + 100));

			m_YAxisMaxSpeed = (cameras[0].m_YAxis.m_MaxSpeed - 1) / 5;
			SetMaxSpeedSliderInitialValues(verticalSliderGamepad, m_YAxisMaxSpeed, verticalSliderGamepadValueText,
			                               ((m_YAxisMaxSpeed * 5) + 1));
		}
		
		protected override void SetYAxisMaxSpeed(CinemachineFreeLook camera, float newMaxYAxisMaxSpeed)
		{
			camera.m_YAxis.m_MaxSpeed = m_YAxisMaxSpeed;
		}

		protected override void SetXAxisMaxSpeed(CinemachineFreeLook camera, float newMaxXAxisMaxSpeed)
		{
			camera.m_XAxis.m_MaxSpeed = m_XAxisMaxSpeed;
		}

		protected override void InvertXAxis(CinemachineFreeLook camera, bool invert)
		{
			camera.m_XAxis.m_InvertInput = invertXAxis;
		}

		protected override void InvertYAxis(CinemachineFreeLook camera, bool invert)
		{
			camera.m_YAxis.m_InvertInput = invertYAxis;
		}
	}
}
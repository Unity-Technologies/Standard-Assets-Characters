using Cinemachine;

namespace Demo
{
	public class VCamPovCameraConfigUi : CameraConfigUi<CinemachineVirtualCamera>
	{
		protected void Start()
		{
			float minSpeed = 100;
			float speedRange = 400;
			
			//SetInitalValues for cameraInversion toggles
			xAxisToggle.isOn = cameras[0].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InvertInput;
			yAxisToggle.isOn = cameras[0].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InvertInput;

			m_XAxisMaxSpeed = (cameras[0].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed - minSpeed) / speedRange;
			
			SetMaxSpeedSliderInitialValues(horizontalSlider, m_XAxisMaxSpeed, horizontalSliderValueText,
				((m_XAxisMaxSpeed * speedRange) + minSpeed));

			m_YAxisMaxSpeed	= (cameras[0].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed - minSpeed) / speedRange;
			SetMaxSpeedSliderInitialValues(verticalSlider, m_YAxisMaxSpeed, verticalSliderValueText,
				((m_YAxisMaxSpeed * speedRange) + minSpeed));
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
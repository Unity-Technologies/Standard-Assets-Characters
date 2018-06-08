using Cinemachine;

namespace Demo
{
	public class VCamPovCameraConfigUi : CameraConfigUi<CinemachineVirtualCamera>
	{
		protected void Start()
		{
		}

		protected override void SetYAxisMaxSpeed(CinemachineVirtualCamera camera, float newMaxYAxisMaxSpeed)
		{
			throw new System.NotImplementedException();
		}

		protected override void SetXAxisMaxSpeed(CinemachineVirtualCamera camera, float newMaxXAxisMaxSpeed)
		{
			throw new System.NotImplementedException();
		}

		protected override void InvertXAxis(CinemachineVirtualCamera camera, bool invert)
		{
			throw new System.NotImplementedException();
		}

		protected override void InvertYAxis(CinemachineVirtualCamera camera, bool invert)
		{
			throw new System.NotImplementedException();
		}
	}
}
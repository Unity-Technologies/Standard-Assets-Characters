using UnityEngine;

namespace StandardAssets.Characters.Common
{
	public class StateDrivenCameraManager:MonoBehaviour
	{
		public CameraAnimationManager cameraAnimationManager;

		private void Awake()
		{
			
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				//Set Strafe Cam
				cameraAnimationManager.SetAnimation("Strafe");
			}

			if (Input.GetKeyDown(KeyCode.G))
			{
				//Set Action Mode 1
				cameraAnimationManager.SetAnimation("Action Mode 1");
			}

			if (Input.GetKeyDown(KeyCode.H))
			{
				//Set Action Mode 2
				cameraAnimationManager.SetAnimation("Action Mode 2");
			}
		}
	}
}
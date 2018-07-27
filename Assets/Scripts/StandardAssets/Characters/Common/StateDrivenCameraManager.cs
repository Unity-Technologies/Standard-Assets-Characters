using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.Common
{
	public class StateDrivenCameraManager:MonoBehaviour
	{
		public CameraAnimationManager cameraAnimationManager;

		[SerializeField]
		protected Text currentCameraText;

		[SerializeField]
		protected InputResponse changeCameraModeInputResponse;

		
		private string[] cameraMode = {"Action Mode 1", "Action Mode 2"};
		private int currentCameraModeIndex = 0;

		private void Awake()
		{
			
			currentCameraText.text = cameraMode[currentCameraModeIndex];
			
			changeCameraModeInputResponse.Init();
			
		}

		private void OnEnable()
		{
			changeCameraModeInputResponse.started += SwitchState;
			
		}

		private void OnDisable()
		{
			changeCameraModeInputResponse.started -= SwitchState;
			
		}

		void StartStrafe()
		{
			//cameraAnimationManager.SetAnimation("Strafe Cam",1);
		}

		void EndStrafe()
		{
			//cameraAnimationManager.SetAnimation("Action Cam",1);
		}

		void SwitchState()
		{
			if (currentCameraModeIndex == 0)
			{
				currentCameraModeIndex = 1;
			}
			else if (currentCameraModeIndex == 1)
			{
				currentCameraModeIndex = 0;
			}

			currentCameraText.text = cameraMode[currentCameraModeIndex];
			cameraAnimationManager.SetAnimation(cameraMode[currentCameraModeIndex]);
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
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using UnityEngine.UI;

namespace StandardAssets.Characters.Common
{
	public class ThirdPersonCameraAnimationManager: CameraAnimationManager
	{
		private string[] actionCameraMode = {"Action Mode 1", "Action Mode 2"};
		private int currentActionModeIndex = 0;

		private string strafeState = "Strafe";
		
		[SerializeField]
		protected InputResponse changeCameraModeInputResponse;
		
		
		//Debug Canvas
		[SerializeField]
		protected Text currentCameraText;
		
		
		private void Awake()
		{
			
			//currentCameraText.text = actionCameraMode[currentActionModeIndex];
			
			changeCameraModeInputResponse.Init();
			
		}

		private void OnEnable()
		{
			changeCameraModeInputResponse.started += SwitchActionMode;
			
		}

		private void OnDisable()
		{
			changeCameraModeInputResponse.started -= SwitchActionMode;
			
		}
		
		public void StrafeStarted()
		{
			SetAnimation(strafeState);
			currentCameraText.text = strafeState;
		}

		public void StrafeEnded()
		{
			SetAnimation(actionCameraMode[currentActionModeIndex]);
			currentCameraText.text = actionCameraMode[currentActionModeIndex];
		}
		
		
		void SwitchActionMode()
		{
			if (currentActionModeIndex == 0)
			{
				currentActionModeIndex = 1;
			}
			else if (currentActionModeIndex == 1)
			{
				currentActionModeIndex = 0;
			}

			currentCameraText.text = actionCameraMode[currentActionModeIndex];
			SetAnimation(actionCameraMode[currentActionModeIndex]);
		}

	}
}
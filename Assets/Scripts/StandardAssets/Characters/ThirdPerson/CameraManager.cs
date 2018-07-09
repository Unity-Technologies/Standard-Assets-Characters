using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class CameraManager : MonoBehaviour
	{
		[SerializeField]
		protected ThirdPersonMotor motor;

		[SerializeField]
		protected GameObject strafeSDC, actionSDC;

		private void OnEnable()
		{
			if (motor == null)
			{
				Debug.LogError("Could not locate Third Person Motor - cannot manage cameras");
				return;
			}
			
			motor.startStrafeMode += MotorOnStartStrafeMode;
			motor.startActionMode += MotorOnStartActionMode;
		}

		private void OnDisable()
		{
			if (motor == null)
			{
				return;
			}
			
			motor.startStrafeMode -= MotorOnStartStrafeMode;
			motor.startActionMode -= MotorOnStartActionMode;
		}
		
		void MotorOnStartActionMode()
		{
			strafeSDC.SetActive(false);
			actionSDC.SetActive(true);
		}

		void MotorOnStartStrafeMode()
		{
			strafeSDC.SetActive(true);
			actionSDC.SetActive(false);
		}
	}
}
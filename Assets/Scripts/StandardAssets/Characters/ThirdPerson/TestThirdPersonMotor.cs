using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class TestThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		public float forwardMovement = 1f;
		
		public float turningSpeed { get; private set; }
		public float lateralSpeed { get; private set; }

		public float forwardSpeed
		{
			get { return forwardMovement; }
		}
	}
}
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public abstract class ThirdPersonMotor : MonoBehaviour 
	{
		#region Properties
		
		public abstract float turningSpeed { get; }
		
		public abstract float lateralSpeed { get; }
		
		public abstract float forwardSpeed { get; }
		
		#endregion
	}
}

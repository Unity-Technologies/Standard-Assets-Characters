using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public interface IThirdPersonMotor 
	{
		#region Properties
		
		float turningSpeed { get; }
		
		float lateralSpeed { get; }
		
		float forwardSpeed { get; }
		
		#endregion
	}
}

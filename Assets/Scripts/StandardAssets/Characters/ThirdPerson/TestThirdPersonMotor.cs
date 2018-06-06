using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// A motor used for testing animations
	/// </summary>
	public class TestThirdPersonMotor : MonoBehaviour, IThirdPersonMotor
	{
		public float forwardMovement = 1f;
		public float lateralMovement;
		
		public float normalizedTurningSpeed { get; private set; }
		
		public float normalizedLateralSpeed 
		{
			get { return lateralMovement; }
		}

		public float normalizedForwardSpeed
		{
			get { return forwardMovement; }
		}

		public float fallTime { get; private set; }

		public Action jumpStarted { get; set; }
		public Action landed { get; set; }
	}
}
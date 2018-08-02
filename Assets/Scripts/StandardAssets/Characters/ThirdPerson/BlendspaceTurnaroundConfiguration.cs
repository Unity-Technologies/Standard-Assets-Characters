using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class BlendspaceTurnaroundConfiguration
	{
		[SerializeField]
		protected float timeToTurn = 0.2f;
		
		[SerializeField]
		protected AnimationCurve rotationDuringTurn = AnimationCurve.Linear(0,0,1,1);

		[SerializeField]
		protected float turnSpeed = 0f;

		[SerializeField]
		protected AnimationCurve forwardSpeed = AnimationCurve.Linear(0, 1, 1, 1);

		[SerializeField]
		protected Calculation forwardSpeedCalculation = Calculation.Multiplicative;

		[SerializeField]
		protected AnimationCurve movementDuringTurn = AnimationCurve.Linear(0, -1, 1, -1);
		
		public float turnTime
		{
			get { return timeToTurn; }
		}

		public float normalizedTurnSpeed
		{
			get { return turnSpeed; }
		}
		
		public AnimationCurve forwardSpeedOverTime
		{
			get { return forwardSpeed; }
		}

		public Calculation forwardSpeedCalc
		{
			get { return forwardSpeedCalculation; }
		}

		public AnimationCurve turnMovementOverTime
		{
			get { return movementDuringTurn; }
		}

		public AnimationCurve rotationOverTime
		{
			get { return rotationDuringTurn; }
		}
	}
}
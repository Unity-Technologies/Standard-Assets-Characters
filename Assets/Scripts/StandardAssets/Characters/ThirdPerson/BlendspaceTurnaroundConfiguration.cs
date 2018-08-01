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
		protected float turnSpeed = 0f;

		[SerializeField]
		protected AnimationCurve forwardSpeed = AnimationCurve.Linear(0, 1, 1, 1);

		[SerializeField]
		protected Calculation forwardSpeedCalculation = Calculation.Multiplicative;

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
	}
}
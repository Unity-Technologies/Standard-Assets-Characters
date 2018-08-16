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
		protected float turnClassificationAngle = 150f;

		[SerializeField]
		protected AnimationCurve movementDuring180Turn = AnimationCurve.Linear(0, 1, 1, 1);
		
		[SerializeField]
		protected AnimationCurve movementDuring90Turn = AnimationCurve.Linear(0, 1, 1, 1);

		[SerializeField]
		protected float headTurnMultiplier = 1f;
		
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

		public float classificationAngle
		{
			get { return turnClassificationAngle; }
		}

		public AnimationCurve turn180MovementOverTime
		{
			get { return movementDuring180Turn; }
		}
		
		public AnimationCurve turn90MovementOverTime
		{
			get { return movementDuring90Turn; }
		}

		public AnimationCurve rotationOverTime
		{
			get { return rotationDuringTurn; }
		}

		public float headTurnScale
		{
			get { return headTurnMultiplier; }
		}
	}
}
using System;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Data class used to store configuration settings used bt <see cref="BlendspaceTurnaroundBehaviour"/>.
	/// </summary>
	[Serializable]
	public class BlendspaceTurnaroundConfiguration
	{
		[SerializeField, Tooltip("Duration of the turnaround.")]
		protected float timeToTurn = 0.2f;
		
		[SerializeField, Tooltip("Curve used to evaluate rotation throughout turnaround.")]
		protected AnimationCurve rotationDuringTurn = AnimationCurve.Linear(0,0,1,1);

		[SerializeField, Tooltip("Curve used to evaluate forward speed throughout turnaround.")]
		protected AnimationCurve forwardSpeed = AnimationCurve.Linear(0, 1, 1, 1);

		[SerializeField, Tooltip("Method to apply forward speed during turnaround.")]
		protected Calculation forwardSpeedCalculation = Calculation.Multiplicative;

		[SerializeField, Tooltip("An angle less than this is classified as a small turn.")]
		protected float turnClassificationAngle = 150f;

		[SerializeField, Tooltip("Curve used to evaluate movement throughout a 180° turnaround.")]
		protected AnimationCurve movementDuring180Turn = AnimationCurve.Linear(0, 1, 1, 1);
		
		[SerializeField, Tooltip("Curve used to evaluate movement throughout a 90° turnaround.")]
		protected AnimationCurve movementDuring90Turn = AnimationCurve.Linear(0, 1, 1, 1);

		[SerializeField, Tooltip("Head look at angle scale during animation.")]
		protected float headTurnMultiplier = 1f;
		
		/// <summary>
		/// Gets the turn duration in seconds.
		/// </summary>
		public float turnTime
		{
			get { return timeToTurn; }
		}
		
		/// <summary>
		/// Gets the curve to evaluate forward speed over time.
		/// </summary>
		public AnimationCurve forwardSpeedOverTime
		{
			get { return forwardSpeed; }
		}

		/// <summary>
		/// Gets the method of applying forward speed.
		/// </summary>
		public Calculation forwardSpeedCalc
		{
			get { return forwardSpeedCalculation; }
		}

		/// <summary>
		/// Gets the angle used for small turn classification.
		/// </summary>
		public float classificationAngle
		{
			get { return turnClassificationAngle; }
		}

		/// <summary>
		/// Gets the curve used to evaluate movement throughout a 180° turnaround.
		/// </summary>
		public AnimationCurve turn180MovementOverTime
		{
			get { return movementDuring180Turn; }
		}
		
		/// <summary>
		/// Gets the curve used to evaluate movement throughout a 90° turnaround.
		/// </summary>
		public AnimationCurve turn90MovementOverTime
		{
			get { return movementDuring90Turn; }
		}

		/// <summary>
		/// Gets the curve used to evaluate rotation over time.
		/// </summary>
		public AnimationCurve rotationOverTime
		{
			get { return rotationDuringTurn; }
		}

		/// <summary>
		/// Gets the head turn scale to be applied during a turnaround.
		/// </summary>
		public float headTurnScale
		{
			get { return headTurnMultiplier; }
		}
	}
}
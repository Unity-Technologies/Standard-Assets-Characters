using System;
using UnityEngine;

namespace Util
{
	/// <summary>
	/// Utility class for evaluating a <see cref="AnimationCurve"/> with a clamp.
	/// </summary>
	[Serializable]
	public class CurveEvaluator
	{
		[SerializeField, Tooltip("Curve used for evaluation")]
		protected AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
		
		[SerializeField, Tooltip("Value used to clamp the curve's output.")]
		protected float maxValue = 1f;

		/// <summary>
		/// Gets the evaluator's maximum value.
		/// </summary>
		public float maximumValue
		{
			get { return maxValue; }
		}

		/// <summary>
		/// Evaluates the curve with the current value, clamping it by <see cref="maxValue"/>.
		/// </summary>
		/// <param name="currentValue">The value used to evaluate the curve.</param>
		/// <returns>The evaluated value.</returns>
		public virtual float Evaluate(float currentValue)
		{
			currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
			return curve.Evaluate(currentValue / maxValue);
		}
	}
}
using System;
using UnityEngine;

namespace Util
{
	/// <inheritdoc />
	[Serializable]
	public class MinMaxCurveEvaluator : CurveEvaluator
	{
		[SerializeField, Tooltip("Value used to clamp the curve's output.")]
		protected float minValue;

		/// <summary>
		/// Evaluates the curve with the current value, clamping it by <see cref="minValue"/> and
		/// <see cref="CurveEvaluator.maxValue"/>
		/// </summary>
		/// <param name="currentValue">The value used to evaluate the curve.</param>
		/// <returns>The evaluated value.</returns>
		public override float Evaluate(float currentValue)
		{
			float fullScale = maxValue - minValue;
			float currentScale = currentValue - minValue;
			return curve.Evaluate(currentScale / fullScale);
		}
	}
}
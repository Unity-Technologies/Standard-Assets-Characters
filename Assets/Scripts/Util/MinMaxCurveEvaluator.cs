using System;
using UnityEngine;

namespace Util
{
	[Serializable]
	public class MinMaxCurveEvaluator : CurveEvaluator
	{
		public float minValue = 0f;

		public override float Evaluate(float currentValue)
		{
			float fullScale = maxValue - minValue;
			float currentScale = currentValue - minValue;
			return curve.Evaluate(currentScale / fullScale);
		}
	}
}
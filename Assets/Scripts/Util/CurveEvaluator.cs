using System;
using UnityEngine;

namespace Util
{
	/// <summary>
	/// Utility for combining curves and values
	/// </summary>
	[Serializable]
	public class CurveEvaluator
	{
		public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
		public float maxValue = 1f;
		
		public virtual float Evaluate(float currentValue)
		{
			currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
			return curve.Evaluate(currentValue / maxValue);
		}
	}
}
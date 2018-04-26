using System;
using UnityEngine;

namespace Util
{
	[Serializable]
	public class CurveEvaluator
	{
		public AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
		public float maxValue = 1f;
		
		public virtual float Evaluate(float currentValue)
		{
			return curve.Evaluate(currentValue / maxValue);
		}
	}
}
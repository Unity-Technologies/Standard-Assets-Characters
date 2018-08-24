using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
	public class SlidingAverage
	{
		private SizedQueue<float> values;

		private readonly int windowSize;

		public float average
		{
			get
			{
				int count = values.Count;
				if (count == 0)
				{
					return 0f;
				}

				float sum = 0f;
				foreach (float value in values.values)
				{
					sum += value;
				}

				return sum / count;
			}
		}

		public SlidingAverage(int setWindowSize)
		{
			values = new SizedQueue<float>(setWindowSize);
		}

		public void Add(float newValue, HandleNegative handleNegative = HandleNegative.Add)
		{
			switch (handleNegative)
			{
				case HandleNegative.Add:
					values.Add(newValue);
					break;
				case HandleNegative.Absolute:
					values.Add(Mathf.Abs(newValue));
					break;
				case HandleNegative.Ignore:
					if (newValue >= 0)
					{
						values.Add(newValue);
					}

					break;
				default:
					throw new ArgumentOutOfRangeException("handleNegative", handleNegative, null);
			}
		}

		public void Clear()
		{
			values.Clear();
		}
	}

	public enum HandleNegative
	{
		Add,
		Ignore,
		Absolute
	}
}
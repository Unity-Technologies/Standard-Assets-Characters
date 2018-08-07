using System.Collections.Generic;

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

		public void Add(float newValue)
		{
			values.Add(newValue);
		}
	}
}
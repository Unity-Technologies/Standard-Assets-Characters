using System.Collections.Generic;

namespace Util
{
	public class SlidingAverage
	{
		private Queue<float> values;

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
				foreach (float value in values)
				{
					sum += value;
				}

				return sum / count;
			}
		}

		public SlidingAverage(int setWindowSize)
		{
			values = new Queue<float>();

			if (setWindowSize <= 0)
			{
				setWindowSize = 1;
			}

			windowSize = setWindowSize;
		}

		public void Add(float newValue)
		{
			values.Enqueue(newValue);
			if (values.Count > windowSize)
			{
				values.Dequeue();
			}
		}
	}
}
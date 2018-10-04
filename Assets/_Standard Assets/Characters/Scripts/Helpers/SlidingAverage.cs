using System;
using UnityEngine;

namespace StandardAssets.Characters.Helpers
{
	/// <summary>
	/// Class that uses a <see cref="SizedQueue{T}"/> to store a set number of values so that an average can be requested.
	/// </summary>
	public class SlidingAverage
	{
		private SizedQueue<float> values;

		private readonly int windowSize;

		/// <summary>
		/// Gets the current average.
		/// </summary>
		public float average
		{
			get
			{
				int count = values.count;
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

		/// <summary>
		/// Adds a new value to the current values.
		/// </summary>
		/// <param name="newValue">The new value to add,</param>
		/// <param name="handleNegative">Describes how a negative value should be handled.</param>
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

		/// <summary>
		/// Clears the values.
		/// </summary>
		public void Clear()
		{
			values.Clear();
		}
	}

	/// <summary>
	/// An enum that describes how a <see cref="SlidingAverage"/> should handle negative values.
	/// </summary>
	public enum HandleNegative
	{
		Add,
		Ignore,
		Absolute
	}
}
using System;
using UnityEngine;

namespace StandardAssets.Characters.Helpers
{
	/// <summary>
	/// Class that uses a <see cref="SizedQueue{T}"/> to store a set number of values so that an average can be requested.
	/// </summary>
	public class SlidingAverage
	{
		// SizeQueue used track averages		
		SizedQueue<float> m_Values;

		// Size of the SizedQueue
		readonly int m_WindowSize;

		/// <summary>
		/// Gets the current average.
		/// </summary>
		public float average
		{
			get
			{
				var count = m_Values.count;
				if (count == 0)
				{
					return 0f;
				}

				var sum = 0f;
				foreach (var value in m_Values.values)
				{
					sum += value;
				}

				return sum / count;
			}
		}

		/// <summary>
		/// Creates a <see cref="SizedQueue{T}"/> of specified size
		/// </summary>
		public SlidingAverage(int setWindowSize)
		{
			m_Values = new SizedQueue<float>(setWindowSize);
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
					m_Values.Add(newValue);
					break;
				case HandleNegative.Absolute:
					m_Values.Add(Mathf.Abs(newValue));
					break;
				case HandleNegative.Ignore:
					if (newValue >= 0)
					{
						m_Values.Add(newValue);
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
			m_Values.Clear();
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
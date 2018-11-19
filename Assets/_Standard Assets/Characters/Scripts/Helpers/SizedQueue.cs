using System.Collections.Generic;

namespace StandardAssets.Characters.Helpers
{
	/// <summary>
	/// A class that wraps a <see cref="Queue{T}"/> and adds an immutable size. The queue is dequeued if a value is
	/// added so that the count exceeds the window size.
	/// </summary>
	public class SizedQueue<T>
	{
		// The maximum number of elements in the Queue
		readonly int m_WindowSize;
		
		/// <summary>
		/// Gets the values of the queue.
		/// </summary>
		public Queue<T> values { get; private set; }
		
		/// <summary>
		/// Gets the count of <see cref="values"/>.
		/// </summary>
		public int count { get { return values.Count; } }
		

		/// <summary>
		/// Initialize the queue with a given window size
		/// </summary>
		public SizedQueue(int size)
		{
			m_WindowSize = size;
			if (m_WindowSize < 1)
			{
				m_WindowSize = 1;
			}
			
			values = new Queue<T>();
		}
		
		/// <summary>
		/// Adds a value to the queue.
		/// </summary>
		/// <param name="newValue">The value to add.</param>
		public void Add(T newValue)
		{
			values.Enqueue(newValue);
			if (values.Count > m_WindowSize)
			{
				values.Dequeue();
			}
		}

		/// <summary>
		/// Clears the queue.
		/// </summary>
		public void Clear()
		{
			values.Clear();
		}
	}
}
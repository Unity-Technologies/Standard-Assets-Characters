using System.Collections.Generic;

namespace Util
{
	public class SizedQueue<T>
	{
		public Queue<T> values { get; protected set; }
		
		private readonly int windowSize;

		public int Count
		{
			get { return values.Count; }
		}

		public SizedQueue(int size)
		{
			windowSize = size;
			if (windowSize < 1)
			{
				windowSize = 1;
			}
			
			values = new Queue<T>();
		}
		
		public void Add(T newValue)
		{
			values.Enqueue(newValue);
			if (values.Count > windowSize)
			{
				values.Dequeue();
			}
		}

		public void Clear()
		{
			values.Clear();
		}
	}
}
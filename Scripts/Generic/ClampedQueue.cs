using System.Collections;
using System.Collections.Generic;

namespace _2HeadedDog
{
	public class ClampedQueue<T>
	{
		public Queue<T> queue;
		public int limit;

		public ClampedQueue(int limit)
		{
			this.limit = limit;
			queue = new Queue<T>(limit);
		}

		public bool Add(T value, out T outValue)
		{
			outValue = default(T);
			bool overflow = false;
			if (queue.Count >= limit)
			{
				outValue = queue.Dequeue();
				overflow = true;
			}
			queue.Enqueue(value);
			return overflow;
		}

		public bool Contains(T value)
		{
			return queue.Contains(value);
		}
	}
}
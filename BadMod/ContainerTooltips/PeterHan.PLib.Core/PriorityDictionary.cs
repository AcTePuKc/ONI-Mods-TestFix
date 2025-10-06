using System;

namespace PeterHan.PLib.Core;

public sealed class PriorityDictionary<K, V> : PriorityQueue<PriorityDictionary<K, V>.PriorityQueuePair> where K : IComparable<K>
{
	public sealed class PriorityQueuePair : IComparable<PriorityQueuePair>
	{
		public K Key { get; }

		public V Value { get; }

		public PriorityQueuePair(K key, V value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			Key = key;
			Value = value;
		}

		public int CompareTo(PriorityQueuePair other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			return Key.CompareTo(other.Key);
		}

		public override bool Equals(object obj)
		{
			if (obj is PriorityQueuePair { Key: var key })
			{
				return key.Equals(Key);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
		}

		public override string ToString()
		{
			return "PriorityQueueItem[key=" + Key?.ToString() + ",value=" + Value?.ToString() + "]";
		}
	}

	public PriorityDictionary()
	{
	}

	public PriorityDictionary(int capacity)
		: base(capacity)
	{
	}

	public void Dequeue(out K key, out V value)
	{
		PriorityQueuePair priorityQueuePair = Dequeue();
		key = priorityQueuePair.Key;
		value = priorityQueuePair.Value;
	}

	public void Enqueue(K key, V value)
	{
		Enqueue(new PriorityQueuePair(key, value));
	}

	public void Peek(out K key, out V value)
	{
		PriorityQueuePair priorityQueuePair = Peek();
		key = priorityQueuePair.Key;
		value = priorityQueuePair.Value;
	}
}

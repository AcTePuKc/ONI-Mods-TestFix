using System;
using System.Collections.Generic;

namespace PeterHan.PLib.Core;

public class PriorityQueue<T> where T : IComparable<T>
{
	private readonly IList<T> heap;

	public int Count => heap.Count;

	private static int ChildIndex(int index)
	{
		return 2 * index + 1;
	}

	private static int ParentIndex(int index)
	{
		return (index - 1) / 2;
	}

	public PriorityQueue()
		: this(32)
	{
	}

	public PriorityQueue(int capacity)
	{
		if (capacity < 1)
		{
			throw new ArgumentException("capacity > 0");
		}
		heap = new List<T>(Math.Max(capacity, 8));
	}

	public void Clear()
	{
		heap.Clear();
	}

	public bool Contains(T key)
	{
		return heap.Contains(key);
	}

	public T Dequeue()
	{
		int index = 0;
		int count = heap.Count;
		if (count == 0)
		{
			throw new InvalidOperationException("Queue is empty");
		}
		T result = heap[0];
		heap[0] = heap[--count];
		heap.RemoveAt(count);
		int num;
		while ((num = ChildIndex(index)) < count)
		{
			T value = heap[index];
			T val = heap[num];
			if (num < count - 1)
			{
				T val2 = heap[num + 1];
				if (val.CompareTo(val2) > 0)
				{
					num++;
					val = val2;
				}
			}
			if (value.CompareTo(val) < 0)
			{
				break;
			}
			heap[num] = value;
			heap[index] = val;
			index = num;
		}
		return result;
	}

	public void Enqueue(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		int num = heap.Count;
		heap.Add(item);
		while (num > 0)
		{
			int num2 = ParentIndex(num);
			T value = heap[num];
			T val = heap[num2];
			if (value.CompareTo(val) <= 0)
			{
				heap[num2] = value;
				heap[num] = val;
				num = num2;
				continue;
			}
			break;
		}
	}

	public T Peek()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Queue is empty");
		}
		return heap[0];
	}

	public override string ToString()
	{
		return heap.ToString();
	}
}

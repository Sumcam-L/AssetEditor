using System;
using System.Collections.Generic;

namespace Sce.Atf.Collections;

public class PriorityQueue<T, TPriority>
{
	private readonly List<KeyValuePair<T, TPriority>> m_heap = new List<KeyValuePair<T, TPriority>>();

	private readonly Dictionary<T, int> m_indexes = new Dictionary<T, int>();

	private readonly IComparer<TPriority> m_comparer;

	private readonly bool m_invert;

	public TPriority this[T item]
	{
		get
		{
			return m_heap[m_indexes[item]].Value;
		}
		set
		{
			if (m_indexes.TryGetValue(item, out var value2))
			{
				int num = m_comparer.Compare(value, m_heap[value2].Value);
				if (num != 0)
				{
					if (m_invert)
					{
						num = ~num;
					}
					KeyValuePair<T, TPriority> element = new KeyValuePair<T, TPriority>(item, value);
					if (num < 0)
					{
						MoveUp(element, value2);
					}
					else
					{
						MoveDown(element, value2);
					}
				}
			}
			else
			{
				KeyValuePair<T, TPriority> keyValuePair = new KeyValuePair<T, TPriority>(item, value);
				m_heap.Add(keyValuePair);
				MoveUp(keyValuePair, Count);
			}
		}
	}

	public int Count => m_heap.Count - 1;

	public PriorityQueue()
		: this(false)
	{
	}

	public PriorityQueue(bool invert)
		: this((IComparer<TPriority>)Comparer<TPriority>.Default)
	{
		m_invert = invert;
	}

	public PriorityQueue(IComparer<TPriority> comparer)
	{
		m_comparer = comparer;
		m_heap.Add(default(KeyValuePair<T, TPriority>));
	}

	public void Enqueue(T item, TPriority priority)
	{
		KeyValuePair<T, TPriority> keyValuePair = new KeyValuePair<T, TPriority>(item, priority);
		m_heap.Add(keyValuePair);
		MoveUp(keyValuePair, Count);
	}

	public KeyValuePair<T, TPriority> Dequeue()
	{
		int count = Count;
		if (count < 1)
		{
			throw new InvalidOperationException("Queue is empty.");
		}
		KeyValuePair<T, TPriority> result = m_heap[1];
		KeyValuePair<T, TPriority> element = m_heap[count];
		m_heap.RemoveAt(count);
		if (count > 1)
		{
			MoveDown(element, 1);
		}
		m_indexes.Remove(result.Key);
		return result;
	}

	public KeyValuePair<T, TPriority> Peek()
	{
		if (Count < 1)
		{
			throw new InvalidOperationException("Queue is empty.");
		}
		return m_heap[1];
	}

	public bool TryGetValue(T item, out TPriority priority)
	{
		if (m_indexes.TryGetValue(item, out var _))
		{
			priority = m_heap[m_indexes[item]].Value;
			return true;
		}
		priority = default(TPriority);
		return false;
	}

	private void MoveUp(KeyValuePair<T, TPriority> element, int index)
	{
		while (index > 1)
		{
			int num = index >> 1;
			if (IsPrior(m_heap[num], element))
			{
				break;
			}
			m_heap[index] = m_heap[num];
			m_indexes[m_heap[num].Key] = index;
			index = num;
		}
		m_heap[index] = element;
		m_indexes[element.Key] = index;
	}

	private void MoveDown(KeyValuePair<T, TPriority> element, int index)
	{
		int count = m_heap.Count;
		while (index << 1 < count)
		{
			int num = index << 1;
			int num2 = num | 1;
			if (num2 < count && IsPrior(m_heap[num2], m_heap[num]))
			{
				num = num2;
			}
			if (IsPrior(element, m_heap[num]))
			{
				break;
			}
			m_heap[index] = m_heap[num];
			m_indexes[m_heap[num].Key] = index;
			index = num;
		}
		m_heap[index] = element;
		m_indexes[element.Key] = index;
	}

	private bool IsPrior(KeyValuePair<T, TPriority> element1, KeyValuePair<T, TPriority> element2)
	{
		int num = m_comparer.Compare(element1.Value, element2.Value);
		if (m_invert)
		{
			num = ~num;
		}
		return num < 0;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf;

[Serializable]
public class IntSet : ICollection<int>, IEnumerable<int>, IEnumerable
{
	public class Range
	{
		private int m_min;

		private int m_max;

		private int m_previousItemsCount;

		public int Min
		{
			get
			{
				return m_min;
			}
			internal set
			{
				m_min = value;
			}
		}

		public int Max
		{
			get
			{
				return m_max;
			}
			internal set
			{
				m_max = value;
			}
		}

		public int Count => m_max - m_min + 1;

		public int PreviousItemsCount
		{
			get
			{
				return m_previousItemsCount;
			}
			internal set
			{
				m_previousItemsCount = value;
			}
		}

		public Range(int value)
		{
			m_min = value;
			m_max = value;
			m_previousItemsCount = 0;
		}

		public Range(int min, int max)
		{
			m_min = min;
			m_max = max;
			m_previousItemsCount = 0;
		}

		public bool Contains(int value)
		{
			return value >= m_min && value <= m_max;
		}
	}

	private class RangeIterator : IEnumerator<int>, IDisposable, IEnumerator
	{
		private readonly IntSet m_set;

		private int m_rangeIndex;

		private int m_current;

		public int Current
		{
			get
			{
				if (m_rangeIndex < 0)
				{
					throw new InvalidOperationException("Enumerator not valid");
				}
				return m_current;
			}
		}

		object IEnumerator.Current => Current;

		public RangeIterator(IntSet set)
		{
			m_set = set;
			m_rangeIndex = -1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (m_rangeIndex < 0)
			{
				if (m_set.m_ranges.Count == 0)
				{
					return false;
				}
				m_rangeIndex = 0;
				m_current = m_set.m_ranges[m_rangeIndex].Min;
			}
			else
			{
				Range range = m_set.m_ranges[m_rangeIndex];
				if (range.Max == m_current)
				{
					m_rangeIndex++;
					if (m_set.m_ranges.Count == m_rangeIndex)
					{
						m_rangeIndex = -1;
						return false;
					}
					m_current = m_set.m_ranges[m_rangeIndex].Min;
				}
				else
				{
					m_current++;
				}
			}
			return true;
		}

		public void Reset()
		{
			m_rangeIndex = -1;
		}
	}

	private readonly List<Range> m_ranges = new List<Range>();

	private int m_count;

	private bool m_dirtyRanges;

	private bool m_locked;

	public int Count => m_count;

	public bool IsReadOnly => m_locked;

	public IEnumerable<Range> Ranges
	{
		get
		{
			UpdateRangeCounts();
			return m_ranges;
		}
	}

	public void Add(int item)
	{
		AttemptModify();
		int num = FindRange(item);
		if (num < 0)
		{
			int insertionIndex = -num - 1;
			AddNonOverlappingRange(insertionIndex, item, item);
		}
	}

	public void AddRange(int begin, int end)
	{
		AttemptModify();
		if (end < begin)
		{
			throw new ArgumentException("the end of the range must be >= the beginning");
		}
		FindRanges(begin, end, out var rangeBeginIndex, out var rangeEndIndex, out var newRangeIndex);
		if (rangeBeginIndex >= 0)
		{
			newRangeIndex = rangeBeginIndex;
			begin = Math.Min(begin, m_ranges[rangeBeginIndex].Min);
			end = Math.Max(end, m_ranges[rangeEndIndex].Max);
			RemoveRangeIndices(rangeBeginIndex, rangeEndIndex);
		}
		AddNonOverlappingRange(newRangeIndex, begin, end);
	}

	public bool Remove(int item)
	{
		AttemptModify();
		int num = FindRange(item);
		if (num < 0)
		{
			return false;
		}
		Range range = m_ranges[num];
		if (range.Min == item)
		{
			if (range.Min != range.Max)
			{
				range.Min++;
			}
			else
			{
				m_ranges.RemoveAt(num);
			}
		}
		else if (range.Max == item)
		{
			if (range.Max != range.Min)
			{
				range.Max--;
			}
			else
			{
				m_ranges.RemoveAt(num);
			}
		}
		else
		{
			m_ranges.Insert(num + 1, new Range(item + 1, range.Max));
			range.Max = item - 1;
		}
		m_count--;
		return true;
	}

	public void RemoveRange(int begin, int end)
	{
		AttemptModify();
		if (end < begin)
		{
			throw new ArgumentException("the end of the range must be >= the beginning");
		}
		FindRanges(begin, end, out var rangeBeginIndex, out var rangeEndIndex, out var _);
		if (rangeBeginIndex >= 0)
		{
			int num = -1;
			Range range = m_ranges[rangeBeginIndex];
			if (range.Min < begin)
			{
				num = range.Min;
			}
			int num2 = -1;
			Range range2 = m_ranges[rangeEndIndex];
			if (end < range2.Max)
			{
				num2 = range2.Max;
			}
			RemoveRangeIndices(rangeBeginIndex, rangeEndIndex);
			if (num2 >= 0)
			{
				AddNonOverlappingRange(rangeBeginIndex, end + 1, num2);
			}
			if (num >= 0)
			{
				AddNonOverlappingRange(rangeBeginIndex, num, begin - 1);
			}
		}
	}

	public void Clear()
	{
		AttemptModify();
		m_ranges.Clear();
		m_count = 0;
	}

	public bool Contains(int item)
	{
		return FindRange(item) >= 0;
	}

	public bool Contains(int item, out int index)
	{
		int num = FindRange(item);
		if (num >= 0)
		{
			UpdateRangeCounts();
			Range range = m_ranges[num];
			index = range.PreviousItemsCount + (item - range.Min);
			return true;
		}
		index = -1;
		return false;
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		int num = 0;
		using IEnumerator<int> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			int current = enumerator.Current;
			array[num++] = current;
		}
	}

	public void Lock()
	{
		m_locked = true;
	}

	public IEnumerator<int> GetEnumerator()
	{
		return new RangeIterator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new RangeIterator(this);
	}

	private int FindRange(int value)
	{
		int num = 0;
		int num2 = m_ranges.Count - 1;
		while (num <= num2)
		{
			int num3 = (num + num2) / 2;
			Range range = m_ranges[num3];
			if (range.Contains(value))
			{
				return num3;
			}
			if (range.Min > value)
			{
				num2 = num3 - 1;
				continue;
			}
			if (range.Max < value)
			{
				num = num3 + 1;
				continue;
			}
			return -num3;
		}
		return -(num + 1);
	}

	private void FindRanges(int begin, int end, out int rangeBeginIndex, out int rangeEndIndex, out int newRangeIndex)
	{
		rangeBeginIndex = -1;
		rangeEndIndex = -1;
		newRangeIndex = 0;
		if (m_ranges.Count == 0)
		{
			return;
		}
		int num = FindRange(begin);
		if (num < 0)
		{
			num = -num - 1;
			if (num >= m_ranges.Count || end < m_ranges[num].Min)
			{
				newRangeIndex = num;
				return;
			}
		}
		rangeBeginIndex = num;
		int num2 = FindRange(end);
		if (num2 < 0)
		{
			num2 = -num2 - 1 - 1;
		}
		rangeEndIndex = num2;
		newRangeIndex = -1;
	}

	private void RemoveRangeIndices(int rangeBeginIndex, int rangeEndIndex)
	{
		int num = 0;
		for (int i = rangeBeginIndex; i <= rangeEndIndex; i++)
		{
			num += m_ranges[i].Count;
		}
		m_count -= num;
		m_ranges.RemoveRange(rangeBeginIndex, rangeEndIndex - rangeBeginIndex + 1);
	}

	private void AddNonOverlappingRange(int insertionIndex, int begin, int end)
	{
		m_count += end - begin + 1;
		int num = FindRange(begin - 1);
		int num2 = FindRange(end + 1);
		if (num >= 0)
		{
			Range range = m_ranges[num];
			if (num2 >= 0)
			{
				m_ranges[num] = new Range(range.Min, m_ranges[num2].Max);
				m_ranges.RemoveAt(num2);
			}
			else
			{
				m_ranges[num] = new Range(range.Min, end);
			}
		}
		else if (num2 >= 0)
		{
			m_ranges[num2] = new Range(begin, m_ranges[num2].Max);
		}
		else
		{
			m_ranges.Insert(insertionIndex, new Range(begin, end));
		}
	}

	private void UpdateRangeCounts()
	{
		if (m_dirtyRanges)
		{
			int num = 0;
			for (int i = 0; i < m_ranges.Count; i++)
			{
				Range range = m_ranges[i];
				range.PreviousItemsCount = num;
				num += range.Count;
			}
			m_dirtyRanges = false;
		}
	}

	private void AttemptModify()
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException("this set is read-only");
		}
		m_dirtyRanges = true;
	}
}

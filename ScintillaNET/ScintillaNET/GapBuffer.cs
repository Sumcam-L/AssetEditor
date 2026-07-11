using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ScintillaNET;

[DebuggerDisplay("Count = {Count}")]
internal sealed class GapBuffer<T> : IEnumerable<T>, IEnumerable
{
	private T[] buffer;

	private int gapStart;

	private int gapEnd;

	public int Count => buffer.Length - (gapEnd - gapStart);

	public T this[int index]
	{
		get
		{
			if (index < gapStart)
			{
				return buffer[index];
			}
			return buffer[index + (gapEnd - gapStart)];
		}
		set
		{
			if (index >= gapStart)
			{
				index += gapEnd - gapStart;
			}
			buffer[index] = value;
		}
	}

	public void Add(T item)
	{
		Insert(Count, item);
	}

	public void AddRange(ICollection<T> collection)
	{
		InsertRange(Count, collection);
	}

	private void EnsureGapCapacity(int length)
	{
		if (length > gapEnd - gapStart)
		{
			int num = Count + length;
			int num2 = buffer.Length * 2;
			if (num2 < num)
			{
				num2 = num;
			}
			T[] array = new T[num2];
			int num3 = array.Length - (buffer.Length - gapEnd);
			Array.Copy(buffer, 0, array, 0, gapStart);
			Array.Copy(buffer, gapEnd, array, num3, array.Length - num3);
			buffer = array;
			gapEnd = num3;
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Insert(int index, T item)
	{
		PlaceGapStart(index);
		EnsureGapCapacity(1);
		buffer[index] = item;
		gapStart++;
	}

	public void InsertRange(int index, ICollection<T> collection)
	{
		int count = collection.Count;
		if (count > 0)
		{
			PlaceGapStart(index);
			EnsureGapCapacity(count);
			collection.CopyTo(buffer, gapStart);
			gapStart += count;
		}
	}

	private void PlaceGapStart(int index)
	{
		if (index != gapStart)
		{
			if (gapEnd - gapStart == 0)
			{
				gapStart = index;
				gapEnd = index;
			}
			else if (index < gapStart)
			{
				int num = gapStart - index;
				int length = ((gapEnd - gapStart < num) ? (gapEnd - gapStart) : num);
				Array.Copy(buffer, index, buffer, gapEnd - num, num);
				gapStart -= num;
				gapEnd -= num;
				Array.Clear(buffer, index, length);
			}
			else
			{
				int num2 = index - gapStart;
				int num3 = ((index > gapEnd) ? index : gapEnd);
				Array.Copy(buffer, gapEnd, buffer, gapStart, num2);
				gapStart += num2;
				gapEnd += num2;
				Array.Clear(buffer, num3, gapEnd - num3);
			}
		}
	}

	public void RemoveAt(int index)
	{
		PlaceGapStart(index);
		buffer[gapEnd] = default(T);
		gapEnd++;
	}

	public void RemoveRange(int index, int count)
	{
		if (count > 0)
		{
			PlaceGapStart(index);
			Array.Clear(buffer, gapEnd, count);
			gapEnd += count;
		}
	}

	public GapBuffer(int capacity = 0)
	{
		buffer = new T[capacity];
		gapEnd = buffer.Length;
	}
}

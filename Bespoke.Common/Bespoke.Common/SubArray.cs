using System;
using System.Collections;
using System.Collections.Generic;

namespace Bespoke.Common;

public class SubArray<T> : IEnumerable<T>, IEnumerable
{
	private T[] mSource;

	private int mStart;

	private int mLength;

	public int Length => mLength;

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= mLength)
			{
				throw new ArgumentOutOfRangeException();
			}
			return mSource[mStart + index];
		}
	}

	public SubArray(T[] source, int start, int length)
	{
		if (start < 0 || start >= source.Length)
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (length < 0 || length > source.Length - start)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		mSource = source;
		mStart = start;
		mLength = length;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < mLength; i++)
		{
			yield return mSource[mStart + i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public T[] ToArray()
	{
		T[] array = new T[mLength];
		Array.Copy(mSource, mStart, array, 0, mLength);
		return array;
	}
}

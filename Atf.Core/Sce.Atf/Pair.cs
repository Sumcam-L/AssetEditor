using System;
using System.Collections.Generic;

namespace Sce.Atf;

public struct Pair<T1, T2> : IComparable, IComparable<Pair<T1, T2>>
{
	public T1 First;

	public T2 Second;

	private static readonly IComparer<T1> s_comparer1;

	private static readonly IComparer<T2> s_comparer2;

	private static readonly IEqualityComparer<T1> s_equalityComparer1;

	private static readonly IEqualityComparer<T2> s_equalityComparer2;

	public Pair(T1 first, T2 second)
	{
		First = first;
		Second = second;
	}

	public Pair(KeyValuePair<T1, T2> keyValuePair)
	{
		First = keyValuePair.Key;
		Second = keyValuePair.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is Pair<T1, T2> other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Pair<T1, T2> other)
	{
		return s_equalityComparer1.Equals(First, other.First) && s_equalityComparer2.Equals(Second, other.Second);
	}

	public override int GetHashCode()
	{
		int num = ((First == null) ? 1735930254 : First.GetHashCode());
		int num2 = ((Second == null) ? 2136642869 : Second.GetHashCode());
		return num ^ num2;
	}

	public override string ToString()
	{
		return string.Format("{0} {1}", (First == null) ? "null" : First.ToString(), (Second == null) ? "null" : Second.ToString());
	}

	public int CompareTo(Pair<T1, T2> other)
	{
		try
		{
			int num = s_comparer1.Compare(First, other.First);
			if (num != 0)
			{
				return num;
			}
			return s_comparer2.Compare(Second, other.Second);
		}
		catch (ArgumentException)
		{
			throw new NotSupportedException("Can't compare types");
		}
	}

	int IComparable.CompareTo(object obj)
	{
		if (obj is Pair<T1, T2>)
		{
			return CompareTo((Pair<T1, T2>)obj);
		}
		throw new ArgumentException("obj is wrong type");
	}

	public static bool operator ==(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
	{
		return s_equalityComparer1.Equals(pair1.First, pair2.First) && s_equalityComparer2.Equals(pair1.Second, pair2.Second);
	}

	public static bool operator !=(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
	{
		return !(pair1 == pair2);
	}

	public KeyValuePair<T1, T2> ToKeyValuePair()
	{
		return new KeyValuePair<T1, T2>(First, Second);
	}

	public static explicit operator KeyValuePair<T1, T2>(Pair<T1, T2> pair)
	{
		return new KeyValuePair<T1, T2>(pair.First, pair.Second);
	}

	public static explicit operator Pair<T1, T2>(KeyValuePair<T1, T2> keyValuePair)
	{
		return new Pair<T1, T2>(keyValuePair);
	}

	static Pair()
	{
		s_comparer1 = Comparer<T1>.Default;
		s_comparer2 = Comparer<T2>.Default;
		s_equalityComparer1 = EqualityComparer<T1>.Default;
		s_equalityComparer2 = EqualityComparer<T2>.Default;
	}
}

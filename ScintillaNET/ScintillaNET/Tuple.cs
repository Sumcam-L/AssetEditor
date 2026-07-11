using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ScintillaNET;

internal static class Tuple
{
	public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
	{
		return new Tuple<T1, T2>(item1, item2);
	}
}
[DebuggerDisplay("Item1={Item1};Item2={Item2}")]
internal class Tuple<T1, T2> : IFormattable
{
	private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;

	private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;

	public T1 Item1 { get; private set; }

	public T2 Item2 { get; private set; }

	public Tuple(T1 item1, T2 item2)
	{
		Item1 = item1;
		Item2 = item2;
	}

	public override int GetHashCode()
	{
		int num = 0;
		if (Item1 != null)
		{
			num = Item1Comparer.GetHashCode(Item1);
		}
		if (Item2 != null)
		{
			num = (num << 3) ^ Item2Comparer.GetHashCode(Item2);
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Tuple<T1, T2> tuple))
		{
			return false;
		}
		return Item1Comparer.Equals(Item1, tuple.Item1) && Item2Comparer.Equals(Item2, tuple.Item2);
	}

	public override string ToString()
	{
		return ToString(null, CultureInfo.CurrentCulture);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, format ?? "{0},{1}", new object[2] { Item1, Item2 });
	}
}

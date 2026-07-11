using System;
using System.Collections.Generic;

namespace Firaxis.Utility;

public class PathComparer : StringComparer, IEqualityComparer<string>
{
	public static readonly PathComparer PathCompareIgnoreCase = new PathComparer();

	public static readonly PathComparer PathCompareWithCase = new PathComparer(bIgnoreCase: false);

	private readonly bool m_bIgnoreCase;

	public PathComparer()
		: this(bIgnoreCase: true)
	{
	}

	public PathComparer(bool bIgnoreCase)
	{
		m_bIgnoreCase = bIgnoreCase;
	}

	public override int Compare(string x, string y)
	{
		return PathCompareHelper.ComparePaths(x, y, m_bIgnoreCase);
	}

	public override bool Equals(string x, string y)
	{
		return PathCompareHelper.Equals(x, y, m_bIgnoreCase);
	}

	public override int GetHashCode(string obj)
	{
		return PathCompareHelper.GetHashCode(obj, m_bIgnoreCase);
	}
}

using System.IO;
using Firaxis.MathEx;

namespace Firaxis.Utility;

public static class PathCompareHelper
{
	public static bool StartsWith(string fullPath, string startingPath, bool bIgnoreCase)
	{
		if (fullPath.Length < startingPath.Length)
		{
			return false;
		}
		return ComparePaths(fullPath.Substring(0, startingPath.Length), startingPath, bIgnoreCase) == 0;
	}

	public static bool EndsWith(string fullPath, string endingPath, bool bIgnoreCase)
	{
		if (fullPath.Length < endingPath.Length)
		{
			return false;
		}
		return ComparePaths(fullPath.Substring(fullPath.Length - endingPath.Length), endingPath, bIgnoreCase) == 0;
	}

	public static int GetHashCode(string obj, bool bIgnoreCase)
	{
		return (int)FNV1a.HashString32(obj, bIgnoreCase);
	}

	public static bool Equals(string lhs, string rhs, bool bIgnoreCase)
	{
		return ComparePaths(lhs, rhs, bIgnoreCase) == 0;
	}

	public static int ComparePaths(string lhs, string rhs, bool bIgnoreCase)
	{
		if (lhs == rhs)
		{
			return 0;
		}
		if (lhs == null)
		{
			return -1;
		}
		if (rhs == null)
		{
			return 1;
		}
		int length = lhs.Length;
		int length2 = rhs.Length;
		if (length < length2)
		{
			return -1;
		}
		if (length > length2)
		{
			return 1;
		}
		for (int i = 0; i < length; i++)
		{
			char c = lhs[i];
			char c2 = rhs[i];
			if (bIgnoreCase)
			{
				c = char.ToUpperInvariant(c);
				c2 = char.ToUpperInvariant(c2);
			}
			if (c != c2 && ((c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) || (c2 != Path.DirectorySeparatorChar && c2 != Path.AltDirectorySeparatorChar)))
			{
				if (c < c2)
				{
					return -1;
				}
				if (c > c2)
				{
					return 1;
				}
			}
		}
		return 0;
	}
}

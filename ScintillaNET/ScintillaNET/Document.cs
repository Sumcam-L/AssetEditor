using System;

namespace ScintillaNET;

public struct Document
{
	internal IntPtr Value;

	public static readonly Document Empty;

	public override bool Equals(object obj)
	{
		return obj is IntPtr && Value == ((Document)obj).Value;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(Document a, Document b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(Document a, Document b)
	{
		return a.Value != b.Value;
	}
}

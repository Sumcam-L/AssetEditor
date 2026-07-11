using System;

namespace ScintillaNET;

public struct MarkerHandle
{
	internal IntPtr Value;

	public static readonly MarkerHandle Zero;

	public override bool Equals(object obj)
	{
		return obj is IntPtr && Value == ((MarkerHandle)obj).Value;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(MarkerHandle a, MarkerHandle b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(MarkerHandle a, MarkerHandle b)
	{
		return a.Value != b.Value;
	}
}

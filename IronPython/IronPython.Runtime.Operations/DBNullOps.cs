using System;

namespace IronPython.Runtime.Operations;

public static class DBNullOps
{
	public static bool __nonzero__(DBNull value)
	{
		return false;
	}
}

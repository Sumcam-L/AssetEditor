using System;
using System.Diagnostics;

namespace Sce.Atf;

public static class Requires
{
	[DebuggerStepThrough]
	public static void Require<T>(bool truth) where T : Exception
	{
		if (!truth)
		{
			throw (T)Activator.CreateInstance(typeof(T));
		}
	}

	[DebuggerStepThrough]
	public static void Require<T>(bool truth, string message) where T : Exception
	{
		if (!truth)
		{
			throw (T)Activator.CreateInstance(typeof(T), message);
		}
	}

	[DebuggerStepThrough]
	public static void NotNull(object obj, string message)
	{
		Require<ArgumentNullException>(obj != null, message);
	}

	[DebuggerStepThrough]
	public static void NotNullOrEmpty(string stringParameter, string message)
	{
		NotNull(stringParameter, message);
		Require<ArgumentOutOfRangeException>(stringParameter != string.Empty, message);
	}
}

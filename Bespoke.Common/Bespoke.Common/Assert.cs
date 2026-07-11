using System;

namespace Bespoke.Common;

public static class Assert
{
	public static void ParamIsNotNull(string paramName, object param)
	{
		if (param == null)
		{
			throw new ArgumentNullException(paramName);
		}
	}

	public static void ParamIsNotNull(object param)
	{
		if (param == null || (param is string && string.IsNullOrEmpty((string)param)))
		{
			throw new ArgumentNullException();
		}
	}

	public static void IsTrue(bool condition)
	{
		IsTrue(string.Empty, condition);
	}

	public static void IsTrue(string paramName, bool condition)
	{
		if (!condition)
		{
			throw new ArgumentException("Condition false", paramName);
		}
	}

	public static void IsFalse(bool condition)
	{
		IsFalse(string.Empty, condition);
	}

	public static void IsFalse(string paramName, bool condition)
	{
		if (condition)
		{
			throw new ArgumentException("Condition true", paramName);
		}
	}
}

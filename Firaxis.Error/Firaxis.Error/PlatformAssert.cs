namespace Firaxis.Error;

public static class PlatformAssert
{
	public static void If(bool condition)
	{
		if (condition)
		{
			throw new FiraxisAssertException();
		}
	}

	public static void If(bool condition, string message)
	{
		if (condition)
		{
			throw new FiraxisAssertException(message);
		}
	}

	public static void If(bool condition, string message, params object[] args)
	{
		If(condition, string.Format(message, args));
	}
}

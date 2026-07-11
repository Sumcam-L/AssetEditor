using System;
using System.Runtime.Serialization;

namespace Firaxis.Error;

public class FiraxisAssertException : Exception
{
	public FiraxisAssertException()
	{
	}

	public FiraxisAssertException(string message)
		: base(message)
	{
	}

	public FiraxisAssertException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public FiraxisAssertException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

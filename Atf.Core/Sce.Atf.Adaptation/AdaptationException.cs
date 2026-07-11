using System;

namespace Sce.Atf.Adaptation;

public class AdaptationException : Exception
{
	public AdaptationException(string message)
		: base(message)
	{
	}

	public AdaptationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}

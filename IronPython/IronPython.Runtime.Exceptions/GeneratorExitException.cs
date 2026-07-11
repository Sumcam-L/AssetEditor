using System;
using System.Runtime.Serialization;

namespace IronPython.Runtime.Exceptions;

[Serializable]
public sealed class GeneratorExitException : Exception
{
	public GeneratorExitException()
	{
	}

	public GeneratorExitException(string message)
		: base(message)
	{
	}

	public GeneratorExitException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private GeneratorExitException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

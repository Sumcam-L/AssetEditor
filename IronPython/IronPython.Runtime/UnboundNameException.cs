using System;
using System.Runtime.Serialization;

namespace IronPython.Runtime;

[Serializable]
public class UnboundNameException : Exception
{
	public UnboundNameException()
	{
	}

	public UnboundNameException(string msg)
		: base(msg)
	{
	}

	public UnboundNameException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected UnboundNameException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

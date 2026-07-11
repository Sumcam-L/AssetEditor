using System;
using System.Runtime.Serialization;

namespace IronPython.Runtime;

[Serializable]
public class UnboundLocalException : UnboundNameException
{
	public UnboundLocalException()
	{
	}

	public UnboundLocalException(string msg)
		: base(msg)
	{
	}

	public UnboundLocalException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected UnboundLocalException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

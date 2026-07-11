using System;
using System.Runtime.Serialization;

namespace IronPython.Runtime;

[Serializable]
internal class BadSourceException : Exception
{
	internal byte _badByte;

	public BadSourceException(byte b)
	{
		_badByte = b;
	}

	public BadSourceException()
	{
	}

	public BadSourceException(string msg)
		: base(msg)
	{
	}

	public BadSourceException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected BadSourceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

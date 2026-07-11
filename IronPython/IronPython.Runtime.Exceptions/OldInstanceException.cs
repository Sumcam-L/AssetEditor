using System;
using System.Runtime.Serialization;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Exceptions;

[Serializable]
internal class OldInstanceException : Exception, IPythonException
{
	private OldInstance _instance;

	public OldInstance Instance => _instance;

	public OldInstanceException(OldInstance instance)
	{
		_instance = instance;
	}

	public OldInstanceException(string msg)
		: base(msg)
	{
	}

	public OldInstanceException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected OldInstanceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public object ToPythonException()
	{
		return _instance;
	}
}

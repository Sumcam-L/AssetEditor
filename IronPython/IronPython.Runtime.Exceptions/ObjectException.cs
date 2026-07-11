using System;
using System.Runtime.Serialization;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Exceptions;

[Serializable]
internal sealed class ObjectException : Exception, IPythonException
{
	private object _instance;

	private PythonType _type;

	public object Instance => _instance;

	public PythonType Type => _type;

	public ObjectException(PythonType type, object instance)
	{
		_instance = instance;
		_type = type;
	}

	public ObjectException(string msg)
		: base(msg)
	{
	}

	public ObjectException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private ObjectException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public object ToPythonException()
	{
		return this;
	}
}

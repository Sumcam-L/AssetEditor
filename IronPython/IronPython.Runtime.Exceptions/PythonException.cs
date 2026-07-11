using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Exceptions;

[Serializable]
internal class PythonException : Exception, IPythonAwareException
{
	private object _pyExceptionObject;

	private List<DynamicStackFrame> _frames;

	private TraceBack _traceback;

	object IPythonAwareException.PythonException
	{
		get
		{
			return _pyExceptionObject;
		}
		set
		{
			_pyExceptionObject = value;
		}
	}

	List<DynamicStackFrame> IPythonAwareException.Frames
	{
		get
		{
			return _frames;
		}
		set
		{
			_frames = value;
		}
	}

	TraceBack IPythonAwareException.TraceBack
	{
		get
		{
			return _traceback;
		}
		set
		{
			_traceback = value;
		}
	}

	public PythonException()
	{
	}

	public PythonException(string msg)
		: base(msg)
	{
	}

	public PythonException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected PythonException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

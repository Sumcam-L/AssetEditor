using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Exceptions;

[Serializable]
public class UnicodeTranslateException : UnicodeException, IPythonAwareException
{
	private object _pyExceptionObject;

	private List<DynamicStackFrame> _frames;

	private TraceBack _traceback;

	object IPythonAwareException.PythonException
	{
		get
		{
			if (_pyExceptionObject == null)
			{
				PythonExceptions._UnicodeTranslateError unicodeTranslateError = new PythonExceptions._UnicodeTranslateError();
				unicodeTranslateError.InitializeFromClr(this);
				_pyExceptionObject = unicodeTranslateError;
			}
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

	public UnicodeTranslateException()
	{
	}

	public UnicodeTranslateException(string msg)
		: base(msg)
	{
	}

	public UnicodeTranslateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected UnicodeTranslateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("frames", _frames);
		info.AddValue("traceback", _traceback);
		base.GetObjectData(info, context);
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Exceptions;

[Serializable]
public class FutureWarningException : WarningException, IPythonAwareException
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
				PythonExceptions.BaseException ex = new PythonExceptions.BaseException(PythonExceptions.FutureWarning);
				ex.InitializeFromClr(this);
				_pyExceptionObject = ex;
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

	public FutureWarningException()
	{
	}

	public FutureWarningException(string msg)
		: base(msg)
	{
	}

	public FutureWarningException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected FutureWarningException(SerializationInfo info, StreamingContext context)
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

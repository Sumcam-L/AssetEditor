using System;
using System.Collections.Generic;
using IronPython.Modules;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Debugging;

namespace IronPython.Runtime;

internal sealed class PythonTracebackListener : ITraceCallback
{
	private readonly PythonContext _pythonContext;

	[ThreadStatic]
	private static TracebackDelegate _globalTraceDispatch;

	[ThreadStatic]
	private static object _globalTraceObject;

	[ThreadStatic]
	internal static bool InTraceBack;

	private bool _exceptionThrown;

	internal PythonContext PythonContext => _pythonContext;

	internal bool ExceptionThrown => _exceptionThrown;

	internal PythonTracebackListener(PythonContext pythonContext)
	{
		_pythonContext = pythonContext;
	}

	internal static void SetTrace(object function, TracebackDelegate traceDispatch)
	{
		_globalTraceDispatch = traceDispatch;
		_globalTraceObject = function;
	}

	internal static object GetTraceObject()
	{
		return _globalTraceObject;
	}

	public void OnTraceEvent(TraceEventKind kind, string name, string sourceFileName, SourceSpan sourceSpan, Func<IDictionary<object, object>> scopeCallback, object payload, object customPayload)
	{
		if (kind == TraceEventKind.ThreadExit || kind == TraceEventKind.ExceptionUnwind)
		{
			return;
		}
		TracebackDelegate tracebackDelegate = null;
		object obj = null;
		List<FunctionStack> functionStack = PythonOps.GetFunctionStack();
		if (InTraceBack)
		{
			return;
		}
		try
		{
			TraceBackFrame traceBackFrame;
			if (kind == TraceEventKind.FrameEnter)
			{
				tracebackDelegate = _globalTraceDispatch;
				obj = _globalTraceObject;
				PythonDebuggingPayload pythonDebuggingPayload = (PythonDebuggingPayload)customPayload;
				traceBackFrame = new TraceBackFrame(this, pythonDebuggingPayload.Code, (functionStack.Count == 0) ? null : functionStack[functionStack.Count - 1].Frame, pythonDebuggingPayload, scopeCallback);
				functionStack.Add(new FunctionStack(traceBackFrame));
				if (obj == null)
				{
					return;
				}
				traceBackFrame.Setf_trace(obj);
			}
			else
			{
				if (functionStack.Count == 0)
				{
					return;
				}
				traceBackFrame = functionStack[functionStack.Count - 1].Frame;
				if (traceBackFrame == null)
				{
					traceBackFrame = SysModule._getframeImpl(functionStack[functionStack.Count - 1].Context, 0);
				}
				tracebackDelegate = traceBackFrame.TraceDelegate;
				obj = traceBackFrame.Getf_trace();
			}
			if (kind != TraceEventKind.FrameExit)
			{
				traceBackFrame._lineNo = sourceSpan.Start.Line;
			}
			if (obj != null && !_exceptionThrown)
			{
				DispatchTrace(functionStack, kind, payload, tracebackDelegate, obj, traceBackFrame);
			}
		}
		finally
		{
			if (kind == TraceEventKind.FrameExit && functionStack.Count > 0 && functionStack[functionStack.Count - 1].Code == ((PythonDebuggingPayload)customPayload).Code)
			{
				functionStack.RemoveAt(functionStack.Count - 1);
			}
		}
	}

	private void DispatchTrace(List<FunctionStack> thread, TraceEventKind kind, object payload, TracebackDelegate traceDispatch, object traceDispatchObject, TraceBackFrame pyFrame)
	{
		object payload2 = null;
		string result = string.Empty;
		switch (kind)
		{
		case TraceEventKind.FrameEnter:
			result = "call";
			break;
		case TraceEventKind.TracePoint:
			result = "line";
			break;
		case TraceEventKind.Exception:
		{
			result = "exception";
			object obj = PythonExceptions.ToPython((Exception)payload);
			object pythonType = ((IPythonObject)obj).PythonType;
			payload2 = PythonTuple.MakeTuple(pythonType, obj, new TraceBack(null, pyFrame));
			break;
		}
		case TraceEventKind.FrameExit:
			result = "return";
			payload2 = payload;
			break;
		}
		bool flag = true;
		InTraceBack = true;
		try
		{
			TracebackDelegate value = traceDispatch(pyFrame, result, payload2);
			flag = false;
			pyFrame.Setf_trace(value);
		}
		finally
		{
			InTraceBack = false;
			if (flag)
			{
				_globalTraceObject = (_globalTraceDispatch = null);
				_exceptionThrown = true;
			}
		}
	}
}

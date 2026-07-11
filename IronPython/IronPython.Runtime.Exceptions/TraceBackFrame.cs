using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Exceptions;

[Serializable]
[PythonType("frame")]
public class TraceBackFrame
{
	private readonly PythonTracebackListener _traceAdapter;

	private TracebackDelegate _trace;

	private object _traceObject;

	internal int _lineNo;

	private readonly PythonDebuggingPayload _debugProperties;

	private readonly Func<IDictionary<object, object>> _scopeCallback;

	private readonly PythonDictionary _globals;

	private readonly object _locals;

	private readonly FunctionCode _code;

	private readonly CodeContext _context;

	private readonly TraceBackFrame _back;

	internal CodeContext Context => _context;

	internal TracebackDelegate TraceDelegate
	{
		get
		{
			if (_traceAdapter != null)
			{
				return _trace;
			}
			return null;
		}
	}

	public PythonDictionary f_globals
	{
		get
		{
			if (_scopeCallback != null && _scopeCallback().TryGetValue("$globalContext", out var value) && value != null)
			{
				return ((CodeContext)value).GlobalDict;
			}
			return _globals;
		}
	}

	public object f_locals
	{
		get
		{
			if (_traceAdapter != null && _scopeCallback != null)
			{
				if (_code.IsModule)
				{
					return f_globals;
				}
				return new PythonDictionary(new DebuggerDictionaryStorage(_scopeCallback()));
			}
			return _locals;
		}
	}

	public FunctionCode f_code => _code;

	public object f_builtins => PythonContext.GetContext(_context).BuiltinModuleDict;

	public TraceBackFrame f_back => _back;

	public object f_exc_traceback => null;

	public object f_exc_type => null;

	public bool f_restricted => false;

	public object f_lineno
	{
		get
		{
			if (_traceAdapter != null)
			{
				return _lineNo;
			}
			return 1;
		}
		set
		{
			if (!(value is int lineNumber))
			{
				throw PythonOps.ValueError("lineno must be an integer");
			}
			if (_traceAdapter != null)
			{
				SetLineNumber(lineNumber);
				return;
			}
			throw PythonOps.ValueError("f_lineno can only be set by a trace function");
		}
	}

	internal TraceBackFrame(CodeContext context, PythonDictionary globals, object locals, FunctionCode code)
	{
		_globals = globals;
		_locals = locals;
		_code = code;
		_context = context;
	}

	internal TraceBackFrame(CodeContext context, PythonDictionary globals, object locals, FunctionCode code, TraceBackFrame back)
	{
		_globals = globals;
		_locals = locals;
		_code = code;
		_context = context;
		_back = back;
	}

	internal TraceBackFrame(PythonTracebackListener traceAdapter, FunctionCode code, TraceBackFrame back, PythonDebuggingPayload debugProperties, Func<IDictionary<object, object>> scopeCallback)
	{
		_traceAdapter = traceAdapter;
		_code = code;
		_back = back;
		_debugProperties = debugProperties;
		_scopeCallback = scopeCallback;
	}

	[SpecialName]
	[PropertyMethod]
	public object Getf_trace()
	{
		if (_traceAdapter != null)
		{
			return _traceObject;
		}
		return null;
	}

	[SpecialName]
	[PropertyMethod]
	public void Setf_trace(object value)
	{
		_traceObject = value;
		_trace = (TracebackDelegate)Converter.ConvertToDelegate(value, typeof(TracebackDelegate));
	}

	[SpecialName]
	[PropertyMethod]
	public void Deletef_trace()
	{
		Setf_trace(null);
	}

	private void SetLineNumber(int newLineNum)
	{
		List<FunctionStack> functionStackNoCreate = PythonOps.GetFunctionStackNoCreate();
		if (!IsTopMostFrame(functionStackNoCreate))
		{
			if (!TracingThisFrame(functionStackNoCreate))
			{
				throw PythonOps.ValueError("f_lineno can only be set by a trace function");
			}
			return;
		}
		FunctionCode code = _debugProperties.Code;
		Dictionary<int, Dictionary<int, bool>> loopAndFinallyLocations = _debugProperties.LoopAndFinallyLocations;
		Dictionary<int, bool> handlerLocations = _debugProperties.HandlerLocations;
		Dictionary<int, bool> value = null;
		bool flag = loopAndFinallyLocations?.TryGetValue(_lineNo, out value) ?? false;
		int num = newLineNum;
		if (newLineNum < code.Span.Start.Line)
		{
			throw PythonOps.ValueError("line {0} comes before the current code block", newLineNum);
		}
		if (newLineNum > code.Span.End.Line)
		{
			throw PythonOps.ValueError("line {0} comes after the current code block", newLineNum);
		}
		while (newLineNum <= code.Span.End.Line)
		{
			SourceSpan sourceSpan = new SourceSpan(new SourceLocation(0, newLineNum, 1), new SourceLocation(0, newLineNum, int.MaxValue));
			if (handlerLocations != null && handlerLocations.TryGetValue(newLineNum, out var _))
			{
				throw PythonOps.ValueError("can't jump to 'except' line");
			}
			if (loopAndFinallyLocations != null && loopAndFinallyLocations.TryGetValue(newLineNum, out var value3))
			{
				if (!flag)
				{
					throw BadForOrFinallyJump(newLineNum, value3);
				}
				foreach (int key in value3.Keys)
				{
					if (!value.ContainsKey(key))
					{
						throw BadForOrFinallyJump(newLineNum, value);
					}
				}
			}
			else if (value != null)
			{
				foreach (bool value4 in value.Values)
				{
					if (value4)
					{
						throw PythonOps.ValueError("can't jump out of 'finally block'");
					}
				}
			}
			if (_traceAdapter.PythonContext.TracePipeline.CanSetNextStatement(_code.co_filename, sourceSpan))
			{
				_traceAdapter.PythonContext.TracePipeline.SetNextStatement(_code.co_filename, sourceSpan);
				_lineNo = newLineNum;
				return;
			}
			newLineNum++;
		}
		throw PythonOps.ValueError("line {0} is invalid jump location ({1} - {2} are valid)", num, code.Span.Start.Line, code.Span.End.Line);
	}

	private bool TracingThisFrame(List<FunctionStack> pyThread)
	{
		if (pyThread != null)
		{
			return pyThread.FindIndex((FunctionStack x) => x.Frame == this) != -1;
		}
		return false;
	}

	private bool IsTopMostFrame(List<FunctionStack> pyThread)
	{
		if (pyThread != null && pyThread.Count != 0)
		{
			return object.ReferenceEquals(this, pyThread[pyThread.Count - 1].Frame);
		}
		return false;
	}

	private static Exception BadForOrFinallyJump(int newLineNum, Dictionary<int, bool> jumpIntoLoopIds)
	{
		foreach (bool value in jumpIntoLoopIds.Values)
		{
			if (value)
			{
				return PythonOps.ValueError("can't jump into 'finally block'", newLineNum);
			}
		}
		return PythonOps.ValueError("can't jump into 'for loop'", newLineNum);
	}
}

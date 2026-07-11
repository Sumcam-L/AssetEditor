using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("generator")]
[DontMapIDisposableToContextManager]
[DontMapIEnumerableToContains]
public sealed class PythonGenerator : IEnumerator<object>, IDisposable, IEnumerator, ICodeFormattable, IEnumerable, IWeakReferenceable
{
	[Flags]
	private enum GeneratorFlags
	{
		None = 0,
		Closed = 1,
		CanSetSysExcInfo = 4
	}

	private class GeneratorFinalizer
	{
		public PythonGenerator Generator;

		public GeneratorFinalizer(PythonGenerator generator)
		{
			Generator = generator;
		}

		~GeneratorFinalizer()
		{
			PythonGenerator generator = Generator;
			if (generator != null)
			{
				generator._finalizer = null;
				generator.Finalizer();
			}
		}
	}

	private readonly Func<MutableTuple, object> _next;

	private readonly PythonFunction _function;

	private readonly MutableTuple _data;

	private readonly MutableTuple<int, object> _dataTuple;

	private GeneratorFlags _flags;

	private bool _active;

	private GeneratorFinalizer _finalizer;

	private static GeneratorFinalizer _LastFinalizer;

	private object[] _excInfo;

	private object _sendValue;

	private WeakRefTracker _tracker;

	public FunctionCode gi_code => _function.func_code;

	public int gi_running
	{
		get
		{
			if (Active)
			{
				return 1;
			}
			return 0;
		}
	}

	public TraceBackFrame gi_frame => new TraceBackFrame(_function.Context, _function.Context.GlobalDict, new PythonDictionary(), gi_code);

	public string __name__ => _function.__name__;

	private int State
	{
		get
		{
			return _dataTuple.Item000;
		}
		set
		{
			_dataTuple.Item000 = value;
		}
	}

	private object CurrentValue
	{
		get
		{
			return _dataTuple.Item001;
		}
		set
		{
			_dataTuple.Item001 = value;
		}
	}

	internal CodeContext Context => _function.Context;

	internal PythonFunction Function => _function;

	private bool Closed
	{
		get
		{
			return (_flags & GeneratorFlags.Closed) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= GeneratorFlags.Closed;
			}
			else
			{
				_flags &= ~GeneratorFlags.Closed;
			}
		}
	}

	private bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
		}
	}

	internal bool CanSetSysExcInfo => (_function.Flags & FunctionAttributes.CanSetSysExcInfo) != 0;

	internal bool ContainsTryFinally => (_function.Flags & FunctionAttributes.ContainsTryFinally) != 0;

	object IEnumerator.Current => CurrentValue;

	object IEnumerator<object>.Current => CurrentValue;

	internal PythonGenerator(PythonFunction function, Func<MutableTuple, object> next, MutableTuple data)
	{
		_function = function;
		_next = next;
		_data = data;
		_dataTuple = GetDataTuple();
		State = -1;
		if (_LastFinalizer == null || (_finalizer = Interlocked.Exchange(ref _LastFinalizer, null)) == null)
		{
			_finalizer = new GeneratorFinalizer(this);
		}
		else
		{
			_finalizer.Generator = this;
		}
	}

	[LightThrowing]
	public object next()
	{
		if (Closed)
		{
			return LightExceptions.Throw(PythonOps.StopIteration());
		}
		object obj = NextWorker();
		if (obj == OperationFailed.Value)
		{
			return LightExceptions.Throw(PythonOps.StopIteration());
		}
		return obj;
	}

	[LightThrowing]
	public object @throw(object type)
	{
		return @throw(type, null, null);
	}

	[LightThrowing]
	public object @throw(object type, object value)
	{
		return @throw(type, value, null);
	}

	[LightThrowing]
	public object @throw(object type, object value, object traceback)
	{
		if (type == null)
		{
			throw PythonOps.MakeExceptionTypeError(null);
		}
		_excInfo = new object[3] { type, value, traceback };
		if (Closed)
		{
			object obj = CheckThrowable();
			if (obj != null)
			{
				return obj;
			}
		}
		if (!((IEnumerator)this).MoveNext())
		{
			return LightExceptions.Throw(PythonOps.StopIteration());
		}
		return CurrentValue;
	}

	[LightThrowing]
	public object send(object value)
	{
		if (value != null && State == -1)
		{
			throw PythonOps.TypeErrorForIllegalSend();
		}
		_sendValue = value;
		return next();
	}

	[LightThrowing]
	public object close()
	{
		if (Closed)
		{
			return null;
		}
		try
		{
			object exceptionValue = @throw(new GeneratorExitException());
			Exception lightException = LightExceptions.GetLightException(exceptionValue);
			if (lightException != null)
			{
				if (lightException is StopIterationException || lightException is GeneratorExitException)
				{
					return null;
				}
				return lightException;
			}
			return LightExceptions.Throw(new RuntimeException("generator ignored GeneratorExit"));
		}
		catch (StopIterationException)
		{
		}
		catch (GeneratorExitException)
		{
		}
		return null;
	}

	private MutableTuple<int, object> GetDataTuple()
	{
		MutableTuple<int, object> mutableTuple = _data as MutableTuple<int, object>;
		if (mutableTuple == null)
		{
			mutableTuple = GetBigData(_data);
		}
		return mutableTuple;
	}

	private static MutableTuple<int, object> GetBigData(MutableTuple data)
	{
		MutableTuple<int, object> result;
		do
		{
			data = (MutableTuple)data.GetValue(0);
		}
		while ((result = data as MutableTuple<int, object>) == null);
		return result;
	}

	private void Finalizer()
	{
		if (!CanSetSysExcInfo && !ContainsTryFinally)
		{
			return;
		}
		try
		{
			object exceptionValue = close();
			Exception lightException = LightExceptions.GetLightException(exceptionValue);
			if (lightException != null)
			{
				HandleFinalizerException(lightException);
			}
		}
		catch (Exception e)
		{
			HandleFinalizerException(e);
		}
	}

	private void HandleFinalizerException(Exception e)
	{
		try
		{
			string o = "Exception in generator " + __repr__(Context) + " ignored";
			PythonOps.PrintWithDest(Context, PythonContext.GetContext(Context).SystemStandardError, o);
			PythonOps.PrintWithDest(Context, PythonContext.GetContext(Context).SystemStandardError, Context.LanguageContext.FormatException(e));
		}
		catch
		{
		}
	}

	bool IEnumerator.MoveNext()
	{
		if (Closed)
		{
			return false;
		}
		CheckSetActive();
		if (!CanSetSysExcInfo)
		{
			return MoveNextWorker();
		}
		Exception save = SaveCurrentException();
		try
		{
			return MoveNextWorker();
		}
		finally
		{
			RestoreCurrentException(save);
		}
	}

	private bool MoveNextWorker()
	{
		bool flag = false;
		try
		{
			try
			{
				_next(_data);
				flag = State != 0;
			}
			finally
			{
				Active = false;
				if (!flag)
				{
					Close();
				}
			}
		}
		catch (StopIterationException)
		{
			return false;
		}
		return flag;
	}

	private object NextWorker()
	{
		CheckSetActive();
		Exception save = SaveCurrentException();
		bool flag = false;
		try
		{
			if (!(flag = GetNext()))
			{
				CurrentValue = OperationFailed.Value;
			}
		}
		finally
		{
			RestoreCurrentException(save);
			Active = false;
			if (!flag)
			{
				Close();
			}
		}
		return CurrentValue;
	}

	private void RestoreCurrentException(Exception save)
	{
		if (CanSetSysExcInfo)
		{
			PythonOps.RestoreCurrentException(save);
		}
	}

	private Exception SaveCurrentException()
	{
		if (CanSetSysExcInfo)
		{
			return PythonOps.SaveCurrentException();
		}
		return null;
	}

	private void CheckSetActive()
	{
		if (Active)
		{
			AlreadyExecuting();
		}
		Active = true;
	}

	private static void AlreadyExecuting()
	{
		throw PythonOps.ValueError("generator already executing");
	}

	[LightThrowing]
	internal object CheckThrowableAndReturnSendValue()
	{
		if (_sendValue != null)
		{
			return SwapValues();
		}
		return CheckThrowable();
	}

	private object SwapValues()
	{
		object sendValue = _sendValue;
		_sendValue = null;
		return sendValue;
	}

	[LightThrowing]
	private object CheckThrowable()
	{
		if (_excInfo != null)
		{
			return ThrowThrowable();
		}
		return null;
	}

	[LightThrowing]
	private object ThrowThrowable()
	{
		object[] excInfo = _excInfo;
		_excInfo = null;
		return LightExceptions.Throw(PythonOps.MakeException(Context, excInfo[0], excInfo[1], excInfo[2]));
	}

	private void Close()
	{
		Closed = true;
		SuppressFinalize();
	}

	private void SuppressFinalize()
	{
		if (_finalizer != null)
		{
			_finalizer.Generator = null;
			_LastFinalizer = _finalizer;
		}
	}

	private bool GetNext()
	{
		_next(_data);
		return State != 0;
	}

	public string __repr__(CodeContext context)
	{
		return $"<generator object at {PythonOps.HexId(this)}>";
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}

	void IDisposable.Dispose()
	{
		SuppressFinalize();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this;
	}

	WeakRefTracker IWeakReferenceable.GetWeakRef()
	{
		return _tracker;
	}

	bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
	{
		_tracker = value;
		return true;
	}

	void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
	{
		_tracker = value;
	}
}

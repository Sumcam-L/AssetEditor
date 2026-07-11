using System;
using System.Collections.Generic;
using IronPython.Modules;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class SysModuleDictionaryStorage : ModuleDictionaryStorage
{
	[Flags]
	private enum ExceptionStateFlags
	{
		Type = 1,
		Value = 2,
		Traceback = 4
	}

	private class ExceptionState
	{
		public readonly object Type;

		public readonly object Value;

		public readonly List<DynamicStackFrame> Traceback;

		public readonly int FrameCount;

		public readonly Exception ClrException;

		public ExceptionState(Exception clrException, object type, object value, List<DynamicStackFrame> traceback)
		{
			Type = type;
			Value = value;
			Traceback = traceback;
			ClrException = clrException;
			if (traceback != null)
			{
				FrameCount = traceback.Count;
			}
		}
	}

	private ExceptionState _exceptionState;

	private object _excType;

	private object _excValue;

	private object _excTraceback;

	private ExceptionStateFlags _setValues = ExceptionStateFlags.Type | ExceptionStateFlags.Value | ExceptionStateFlags.Traceback;

	private ExceptionStateFlags _removedValues;

	public SysModuleDictionaryStorage()
		: base(typeof(SysModule))
	{
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		lock (this)
		{
			AddNoLock(ref storage, key, value);
		}
	}

	public override void AddNoLock(ref DictionaryStorage storage, object key, object value)
	{
		if (key is string text)
		{
			switch (text)
			{
			case "exc_type":
				_setValues |= ExceptionStateFlags.Type;
				_removedValues &= ~ExceptionStateFlags.Type;
				_excType = value;
				break;
			case "exc_value":
				_setValues |= ExceptionStateFlags.Value;
				_removedValues &= ~ExceptionStateFlags.Value;
				_excValue = value;
				break;
			case "exc_traceback":
				_setValues |= ExceptionStateFlags.Traceback;
				_removedValues &= ~ExceptionStateFlags.Traceback;
				_excTraceback = value;
				break;
			}
		}
		base.AddNoLock(ref storage, key, value);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		ExceptionState exceptionState = _exceptionState;
		if ((exceptionState != null || _setValues != 0) && key is string text)
		{
			switch (text)
			{
			case "exc_type":
				lock (this)
				{
					_excType = null;
					_setValues &= ~ExceptionStateFlags.Type;
					_removedValues |= ExceptionStateFlags.Type;
				}
				break;
			case "exc_value":
				lock (this)
				{
					_excValue = null;
					_setValues &= ~ExceptionStateFlags.Value;
					_removedValues |= ExceptionStateFlags.Value;
				}
				break;
			case "exc_traceback":
				lock (this)
				{
					_excTraceback = null;
					_setValues &= ~ExceptionStateFlags.Traceback;
					_removedValues |= ExceptionStateFlags.Traceback;
				}
				break;
			}
		}
		return base.Remove(ref storage, key);
	}

	public override bool TryGetValue(object key, out object value)
	{
		ExceptionState exceptionState = _exceptionState;
		if ((exceptionState != null || _setValues != 0) && key is string strKey && TryGetExcValue(exceptionState, strKey, out value))
		{
			return true;
		}
		return base.TryGetValue(key, out value);
	}

	private bool TryGetExcValue(ExceptionState exState, string strKey, out object value)
	{
		switch (strKey)
		{
		case "exc_type":
			lock (this)
			{
				if ((_removedValues & ExceptionStateFlags.Type) == 0)
				{
					if ((_setValues & ExceptionStateFlags.Type) != 0)
					{
						value = _excType;
					}
					else
					{
						value = exState.Type;
					}
					return true;
				}
			}
			break;
		case "exc_value":
			lock (this)
			{
				if ((_removedValues & ExceptionStateFlags.Value) == 0)
				{
					if ((_setValues & ExceptionStateFlags.Value) != 0)
					{
						value = _excValue;
					}
					else
					{
						value = exState.Value;
					}
					return true;
				}
			}
			break;
		case "exc_traceback":
			lock (this)
			{
				if ((_removedValues & ExceptionStateFlags.Traceback) == 0)
				{
					if ((_setValues & ExceptionStateFlags.Traceback) != 0)
					{
						value = _excTraceback;
					}
					else
					{
						_excTraceback = CreateTraceBack(exState);
						_setValues |= ExceptionStateFlags.Traceback;
						value = _excTraceback;
					}
					return true;
				}
			}
			break;
		}
		value = null;
		return false;
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> items = base.GetItems();
		if (TryGetValue("exc_traceback", out var value))
		{
			items.Add(new KeyValuePair<object, object>("exc_traceback", value));
		}
		if (TryGetValue("exc_type", out value))
		{
			items.Add(new KeyValuePair<object, object>("exc_type", value));
		}
		if (TryGetValue("exc_value", out value))
		{
			items.Add(new KeyValuePair<object, object>("exc_value", value));
		}
		return items;
	}

	private static object CreateTraceBack(ExceptionState list)
	{
		return PythonOps.CreateTraceBack(list.ClrException, list.Traceback, list.FrameCount);
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		lock (this)
		{
			_exceptionState = null;
			_setValues = (ExceptionStateFlags)0;
			_removedValues = (ExceptionStateFlags)0;
			_excTraceback = (_excType = (_excValue = null));
			base.Clear(ref storage);
		}
	}

	public void UpdateExceptionInfo(Exception clrException, object type, object value, List<DynamicStackFrame> traceback)
	{
		lock (this)
		{
			_exceptionState = new ExceptionState(clrException, type, value, traceback);
			_setValues = (_removedValues = (ExceptionStateFlags)0);
		}
	}

	public void UpdateExceptionInfo(object type, object value, object traceback)
	{
		lock (this)
		{
			_exceptionState = new ExceptionState(null, type, value, null);
			_excTraceback = traceback;
			_setValues = ExceptionStateFlags.Traceback;
			_removedValues = (ExceptionStateFlags)0;
		}
	}

	public void ExceptionHandled()
	{
		lock (this)
		{
			_setValues = ExceptionStateFlags.Type | ExceptionStateFlags.Value | ExceptionStateFlags.Traceback;
			_removedValues = (ExceptionStateFlags)0;
			_exceptionState = null;
			_excTraceback = (_excType = (_excValue = null));
		}
	}

	public override void Reload()
	{
		base.Reload();
	}
}

using System;
using System.Collections;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("enumerator")]
public class PythonEnumerator : IEnumerator
{
	private readonly object _baseObject;

	private object _nextMethod;

	private object _current;

	public object Current => _current;

	public static bool TryCastIEnumer(object baseObject, out IEnumerator enumerator)
	{
		if (baseObject is IEnumerator)
		{
			enumerator = (IEnumerator)baseObject;
			return true;
		}
		if (baseObject is IEnumerable)
		{
			enumerator = ((IEnumerable)baseObject).GetEnumerator();
			return true;
		}
		enumerator = null;
		return false;
	}

	public static bool TryCreate(object baseObject, out IEnumerator enumerator)
	{
		if (TryCastIEnumer(baseObject, out enumerator))
		{
			return true;
		}
		if (PythonOps.TryGetBoundAttr(baseObject, "__iter__", out var ret))
		{
			object obj = PythonCalls.Call(ret);
			if (TryCastIEnumer(obj, out enumerator))
			{
				return true;
			}
			enumerator = new PythonEnumerator(obj);
			return true;
		}
		enumerator = null;
		return false;
	}

	public static IEnumerator Create(object baseObject)
	{
		if (!TryCreate(baseObject, out var enumerator))
		{
			throw PythonOps.TypeError("cannot convert {0} to IEnumerator", PythonTypeOps.GetName(baseObject));
		}
		return enumerator;
	}

	internal PythonEnumerator(object iter)
	{
		_baseObject = iter;
	}

	public void Reset()
	{
		throw new NotImplementedException();
	}

	public bool MoveNext()
	{
		if (_nextMethod == null && (!PythonOps.TryGetBoundAttr(_baseObject, "next", out _nextMethod) || _nextMethod == null))
		{
			throw PythonOps.TypeError("instance has no next() method");
		}
		try
		{
			_current = DefaultContext.Default.LanguageContext.CallLightEh(DefaultContext.Default, _nextMethod);
			Exception lightException = LightExceptions.GetLightException(_current);
			if (lightException != null)
			{
				if (lightException is StopIterationException)
				{
					return false;
				}
				throw lightException;
			}
			return true;
		}
		catch (StopIterationException)
		{
			return false;
		}
	}

	public object __iter__()
	{
		return this;
	}
}

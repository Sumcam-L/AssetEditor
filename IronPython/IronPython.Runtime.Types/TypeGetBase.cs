using System;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime.Types;

internal abstract class TypeGetBase : FastGetBase
{
	private readonly FastGetDelegate[] _delegates;

	public TypeGetBase(PythonGetMemberBinder binder, FastGetDelegate[] delegates)
	{
		_delegates = delegates;
	}

	protected object RunDelegates(object self, CodeContext context)
	{
		_hitCount++;
		for (int i = 0; i < _delegates.Length; i++)
		{
			if (_delegates[i](context, self, out var result))
			{
				return result;
			}
		}
		throw new InvalidOperationException();
	}

	protected object RunDelegatesNoOptimize(object self, CodeContext context)
	{
		for (int i = 0; i < _delegates.Length; i++)
		{
			if (_delegates[i](context, self, out var result))
			{
				return result;
			}
		}
		throw new InvalidOperationException();
	}
}

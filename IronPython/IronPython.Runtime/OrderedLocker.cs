using System;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public struct OrderedLocker : IDisposable
{
	private readonly object _one;

	private readonly object _two;

	private bool _oneLocked;

	private bool _twoLocked;

	public OrderedLocker(object one, object two)
	{
		_one = one;
		_two = two;
		_oneLocked = false;
		_twoLocked = false;
		if (one == two)
		{
			try
			{
				return;
			}
			finally
			{
				MonitorUtils.Enter(one, ref _oneLocked);
			}
		}
		int hashCode = RuntimeHelpers.GetHashCode(_one);
		int hashCode2 = RuntimeHelpers.GetHashCode(_two);
		if (hashCode < hashCode2)
		{
			MonitorUtils.Enter(_one, ref _oneLocked);
			MonitorUtils.Enter(_two, ref _twoLocked);
		}
		else if (hashCode != hashCode2)
		{
			MonitorUtils.Enter(_two, ref _twoLocked);
			MonitorUtils.Enter(_one, ref _oneLocked);
		}
		else if (IdDispenser.GetId(_one) < IdDispenser.GetId(_two))
		{
			MonitorUtils.Enter(_one, ref _oneLocked);
			MonitorUtils.Enter(_two, ref _twoLocked);
		}
		else
		{
			MonitorUtils.Enter(_two, ref _twoLocked);
			MonitorUtils.Enter(_one, ref _oneLocked);
		}
	}

	public void Dispose()
	{
		MonitorUtils.Exit(_one, ref _oneLocked);
		if (_one != _two)
		{
			MonitorUtils.Exit(_two, ref _twoLocked);
		}
	}
}

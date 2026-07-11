using System;

namespace Firaxis.MVVMBase;

public interface IWeakFunc
{
	bool IsAlive { get; }

	bool Execute(object parameter, out object result);

	void ClearReferences();

	bool FuncEquals(Delegate other);
}

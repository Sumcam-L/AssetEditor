using System;

namespace Firaxis.MVVMBase;

public interface IWeakAction
{
	bool IsAlive { get; }

	bool Execute(object parameter);

	void ClearReferences();

	bool ActionEquals(Delegate other);
}

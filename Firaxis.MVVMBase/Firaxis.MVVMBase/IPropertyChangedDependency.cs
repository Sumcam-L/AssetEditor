using System;
using System.Collections.Generic;

namespace Firaxis.MVVMBase;

public interface IPropertyChangedDependency
{
	WeakAction<string> Action { get; }

	bool CanTrigger(HashSet<string> propertyNames);

	bool AddToPropertyChangedDependency(Action<string> action, string[] propertyNames, out IPropertyChangedDependency resultingPropertyChangedDependency);

	bool RemoveFromPropertyChangedDependency(Action<string> action, string[] propertyNames, out IPropertyChangedDependency resultingPropertyChangedDependency);
}

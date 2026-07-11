using System;
using System.Collections.Generic;

namespace Firaxis.MVVMBase;

public class SinglePropertyChangedDependency : PropertyChangedDependency
{
	public string PropertyName { get; }

	public SinglePropertyChangedDependency(WeakAction<string> action, string propertyName)
		: base(action)
	{
		PropertyName = propertyName;
	}

	public override bool CanTrigger(HashSet<string> propertyNames)
	{
		return propertyNames.Contains(PropertyName);
	}

	public override bool RemoveFromPropertyChangedDependency(Action<string> action, string[] propertyNames, out IPropertyChangedDependency resultingPropertyChangedDependency)
	{
		if (!base.Action.ActionEquals(action))
		{
			resultingPropertyChangedDependency = null;
			return false;
		}
		foreach (string text in propertyNames)
		{
			if (!(PropertyName != text))
			{
				resultingPropertyChangedDependency = null;
				return true;
			}
		}
		resultingPropertyChangedDependency = this;
		return true;
	}
}

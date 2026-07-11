using System;
using System.Collections.Generic;

namespace Firaxis.MVVMBase;

public class MultiPropertyChangedDependency : PropertyChangedDependency
{
	public HashSet<string> PropertyNames { get; }

	public MultiPropertyChangedDependency(WeakAction<string> action, IEnumerable<string> propertyNames)
		: base(action)
	{
		PropertyNames = new HashSet<string>(propertyNames);
	}

	public override bool CanTrigger(HashSet<string> propertyNames)
	{
		foreach (string propertyName in PropertyNames)
		{
			if (propertyNames.Contains(propertyName))
			{
				return true;
			}
		}
		return false;
	}

	public override bool AddToPropertyChangedDependency(Action<string> action, string[] propertyNames, out IPropertyChangedDependency resultingPropertyChangedDependency)
	{
		if (!base.Action.ActionEquals(action))
		{
			resultingPropertyChangedDependency = null;
			return false;
		}
		foreach (string item in propertyNames)
		{
			PropertyNames.Add(item);
		}
		resultingPropertyChangedDependency = this;
		return true;
	}

	public override bool RemoveFromPropertyChangedDependency(Action<string> action, string[] propertyNames, out IPropertyChangedDependency resultingPropertyChangedDependency)
	{
		if (!base.Action.ActionEquals(action))
		{
			resultingPropertyChangedDependency = null;
			return false;
		}
		foreach (string item in propertyNames)
		{
			PropertyNames.Remove(item);
		}
		if (PropertyNames.Count > 0)
		{
			resultingPropertyChangedDependency = this;
		}
		else
		{
			resultingPropertyChangedDependency = null;
		}
		return true;
	}
}

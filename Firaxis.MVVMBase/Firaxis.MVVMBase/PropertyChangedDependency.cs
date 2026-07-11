using System;
using System.Collections.Generic;

namespace Firaxis.MVVMBase;

public class PropertyChangedDependency : IPropertyChangedDependency
{
	public WeakAction<string> Action { get; }

	public PropertyChangedDependency(WeakAction<string> action)
	{
		Action = action;
	}

	public static IPropertyChangedDependency GeneratePropertyChangedDependency(Action<string> action, params string[] triggeringPropertyNames)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (triggeringPropertyNames == null)
		{
			return new PropertyChangedDependency(new WeakAction<string>(action));
		}
		if (triggeringPropertyNames.Length == 1)
		{
			return new SinglePropertyChangedDependency(new WeakAction<string>(action), triggeringPropertyNames[0]);
		}
		return new MultiPropertyChangedDependency(new WeakAction<string>(action), triggeringPropertyNames);
	}

	public virtual bool CanTrigger(HashSet<string> propertyNames)
	{
		return true;
	}

	public virtual bool AddToPropertyChangedDependency(Action<string> action, string[] propertyNames, out IPropertyChangedDependency resultingPropertyChangedDependency)
	{
		if (!Action.ActionEquals(action))
		{
			resultingPropertyChangedDependency = null;
			return false;
		}
		resultingPropertyChangedDependency = GeneratePropertyChangedDependency(action, propertyNames);
		return true;
	}

	public virtual bool RemoveFromPropertyChangedDependency(Action<string> action, string[] propertyNames, out IPropertyChangedDependency resultingPropertyChangedDependency)
	{
		if (!Action.ActionEquals(action))
		{
			resultingPropertyChangedDependency = null;
			return false;
		}
		if (propertyNames == null)
		{
			resultingPropertyChangedDependency = null;
		}
		else
		{
			resultingPropertyChangedDependency = this;
		}
		return true;
	}
}

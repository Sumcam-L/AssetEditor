using System;
using System.Collections.Generic;
using Firaxis.Collections;
using Firaxis.Reflection;

namespace Firaxis.Validation;

public class ValidatorSource : IValidatorSource
{
	private Dictionary<string, ListEvent<IValidator>> validatorMap;

	public IEnumerable<IValidator> Validators
	{
		get
		{
			foreach (KeyValuePair<string, ListEvent<IValidator>> item in validatorMap)
			{
				foreach (IValidator item2 in item.Value)
				{
					yield return item2;
				}
			}
		}
	}

	public event EventHandler<ValidatorSourceEventArgs> ValidationChanged;

	public ValidatorSource()
	{
		validatorMap = new Dictionary<string, ListEvent<IValidator>>();
	}

	public void Rebuild(IEnumerable<ValidatorSourceEntry> entries)
	{
		foreach (ValidatorSourceEntry entry in entries)
		{
			string name = entry.Name;
			if (!validatorMap.TryGetValue(name, out var value))
			{
				value = new ListEvent<IValidator>();
				validatorMap.Add(name, value);
			}
			value.Clear();
			foreach (Type type in entry.Types)
			{
				value.Add((IValidator)ReflectionHelper.CreateInstance(type));
			}
			OnValidationChanged(new ValidatorSourceEventArgs(name));
		}
	}

	public IEnumerable<IValidator> GetValidators(string name)
	{
		if (!validatorMap.TryGetValue(name, out var list))
		{
			yield break;
		}
		foreach (IValidator item in list)
		{
			yield return item;
		}
	}

	protected virtual void OnValidationChanged(ValidatorSourceEventArgs e)
	{
		this.ValidationChanged?.Invoke(this, e);
	}
}

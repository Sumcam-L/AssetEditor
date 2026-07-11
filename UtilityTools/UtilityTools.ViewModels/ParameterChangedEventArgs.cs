using System;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public class ParameterChangedEventArgs : EventArgs
{
	public string EntityName { get; private set; }

	public string ParameterName { get; private set; }

	public IValue NewValue { get; private set; }

	public ParameterChangedEventArgs(string entityName, string paramName, IValue value)
	{
		EntityName = entityName;
		ParameterName = paramName;
		NewValue = value;
	}
}

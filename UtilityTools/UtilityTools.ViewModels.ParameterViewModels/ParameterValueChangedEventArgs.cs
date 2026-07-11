using System;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class ParameterValueChangedEventArgs : EventArgs
{
	public IValue NewValue { get; private set; }

	public string ParameterName { get; private set; }

	public ParameterValueChangedEventArgs(string paramName, IValue newValue)
	{
		ParameterName = paramName;
		NewValue = newValue;
	}
}

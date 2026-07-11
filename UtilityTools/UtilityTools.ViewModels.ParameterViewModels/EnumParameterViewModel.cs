using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class EnumParameterViewModel : BaseParameterViewModel
{
	public IEnumerable<string> Enumerations => EnumParam.GetEnumerations();

	public string StringValue
	{
		get
		{
			return StringValueInternal.ParameterValue;
		}
		set
		{
			if (StringValueInternal.ParameterValue != value)
			{
				StringValueInternal.ParameterValue = value;
				OnPropertyChanged("StringValue");
				OnParameterValueChangedEvent(base.Name, base.Value);
			}
		}
	}

	private IEnumParameter EnumParam => (IEnumParameter)base.Parameter;

	private IStringValue StringValueInternal => (IStringValue)base.Value;

	public EnumParameterViewModel(IEnumParameter param, IStringValue value)
		: base(param, value)
	{
	}
}

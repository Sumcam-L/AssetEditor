using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class StringParameterViewModel : BaseParameterViewModel
{
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

	private IStringParameter StringParam => (IStringParameter)base.Parameter;

	private IStringValue StringValueInternal => (IStringValue)base.Value;

	public StringParameterViewModel(IStringParameter param, IStringValue value)
		: base(param, value)
	{
	}
}

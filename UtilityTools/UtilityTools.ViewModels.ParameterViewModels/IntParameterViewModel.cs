using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class IntParameterViewModel : BaseParameterViewModel
{
	public int IntValue
	{
		get
		{
			return IntValueInternal.ParameterValue;
		}
		set
		{
			if (IntValueInternal.ParameterValue != value && value >= IntParam.Min && value <= IntParam.Max)
			{
				IntValueInternal.ParameterValue = value;
				OnPropertyChanged("IntValue");
				OnParameterValueChangedEvent(base.Name, base.Value);
			}
		}
	}

	public string RangeToolTip
	{
		get
		{
			if (IntParam.Min != IntParam.Max)
			{
				return $"Accepted values: {IntParam.Min} to {IntParam.Max}, inclusive.";
			}
			return null;
		}
	}

	private IIntParameter IntParam => (IIntParameter)base.Parameter;

	private IIntValue IntValueInternal => (IIntValue)base.Value;

	public IntParameterViewModel(IIntParameter param, IIntValue value)
		: base(param, value)
	{
	}
}

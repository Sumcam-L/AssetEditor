using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class BoolParameterViewModel : BaseParameterViewModel
{
	public bool BoolValue
	{
		get
		{
			return BoolValueInternal.ParameterValue;
		}
		set
		{
			if (BoolValueInternal.ParameterValue != value)
			{
				BoolValueInternal.ParameterValue = value;
				OnPropertyChanged("BoolValue");
				OnParameterValueChangedEvent(base.Name, base.Value);
			}
		}
	}

	private IBoolParameter BoolParam => (IBoolParameter)base.Parameter;

	private IBoolValue BoolValueInternal => (IBoolValue)base.Value;

	public BoolParameterViewModel(IBoolParameter param, IBoolValue value)
		: base(param, value)
	{
	}
}

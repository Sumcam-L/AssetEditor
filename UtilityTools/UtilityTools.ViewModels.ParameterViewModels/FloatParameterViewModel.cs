using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class FloatParameterViewModel : BaseParameterViewModel
{
	public float FloatMax => FloatParam.Max;

	public float FloatMin => FloatParam.Min;

	public float FloatValue
	{
		get
		{
			return FloatValueInternal.ParameterValue;
		}
		set
		{
			if (FloatValueInternal.ParameterValue != value && value >= FloatParam.Min && value <= FloatParam.Max)
			{
				FloatValueInternal.ParameterValue = value;
				OnPropertyChanged("FloatValue");
				OnParameterValueChangedEvent(base.Name, base.Value);
			}
		}
	}

	public float LargeChange => (FloatMax - FloatMin) / 100f;

	public float SmallChange => (FloatMax - FloatMin) / 1000f;

	private IFloatParameter FloatParam => (IFloatParameter)base.Parameter;

	private IFloatValue FloatValueInternal => (IFloatValue)base.Value;

	public FloatParameterViewModel(IFloatParameter param, IFloatValue value)
		: base(param, value)
	{
	}
}

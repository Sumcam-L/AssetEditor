using Firaxis.CivTech.AssetPreviewer;

namespace UtilityTools.ViewModels.KnobViewModels;

public class ValueKnobViewModel<T> : KnobViewModel
{
	public virtual bool IsEnabled => !ValueKnob.IsReadOnly;

	public virtual T Value
	{
		get
		{
			return ValueKnob.Value;
		}
		set
		{
			if (!ValueKnob.Value.Equals(value))
			{
				ValueKnob.Value = value;
				OnPropertyChanged("Value");
			}
		}
	}

	private IValueKnob<T> ValueKnob => (IValueKnob<T>)base.Knob;

	public ValueKnobViewModel(IValueKnob<T> knob)
		: base(knob)
	{
	}

	protected override void RaisePropertyChangedEvents()
	{
		base.RaisePropertyChangedEvents();
		OnPropertyChanged("Value");
	}
}

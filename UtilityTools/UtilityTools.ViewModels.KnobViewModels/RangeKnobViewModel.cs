using System;
using Firaxis.CivTech.AssetPreviewer;

namespace UtilityTools.ViewModels.KnobViewModels;

public class RangeKnobViewModel<T> : ValueKnobViewModel<T> where T : IComparable<T>
{
	public T MaxValue => RangeKnob.MaxValue;

	public T MinValue => RangeKnob.MinValue;

	public override T Value
	{
		get
		{
			return base.Value;
		}
		set
		{
			T maxValue = MaxValue;
			if (value.CompareTo(maxValue) > 0)
			{
				value = MaxValue;
			}
			T minValue = MinValue;
			if (value.CompareTo(minValue) < 0)
			{
				value = MinValue;
			}
			base.Value = value;
		}
	}

	private IRangeKnob<T> RangeKnob => (IRangeKnob<T>)base.Knob;

	public RangeKnobViewModel(IRangeKnob<T> knob)
		: base((IValueKnob<T>)knob)
	{
	}

	protected override void RaisePropertyChangedEvents()
	{
		base.RaisePropertyChangedEvents();
		OnPropertyChanged("MaxValue");
		OnPropertyChanged("MinValue");
	}
}

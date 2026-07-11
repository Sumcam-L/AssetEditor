using System.Collections.ObjectModel;
using Firaxis.CivTech.AssetPreviewer;

namespace UtilityTools.ViewModels.KnobViewModels;

public class ContainerKnobViewModel<T> : ValueKnobViewModel<T>
{
	private ObservableCollection<T> m_values;

	public int SelectedValueIndex
	{
		get
		{
			return Values.IndexOf(base.Value);
		}
		set
		{
			if (SelectedValueIndex != value)
			{
				base.Value = Values[value];
				OnPropertyChanged("SelectedValueIndex");
			}
		}
	}

	public override T Value
	{
		get
		{
			return (SelectedValueIndex != -1) ? Values[SelectedValueIndex] : default(T);
		}
		set
		{
			SelectedValueIndex = Values.IndexOf(value);
		}
	}

	public ObservableCollection<T> Values
	{
		get
		{
			return m_values;
		}
		set
		{
			if (m_values != value)
			{
				m_values = value;
				OnPropertyChanged("Values");
			}
		}
	}

	private IContainerKnob<T> ContainerKnob => (IContainerKnob<T>)base.Knob;

	public ContainerKnobViewModel(IContainerKnob<T> knob)
		: base((IValueKnob<T>)knob)
	{
		Values = new ObservableCollection<T>(knob.Values);
	}

	protected override void RaisePropertyChangedEvents()
	{
		Values = new ObservableCollection<T>(ContainerKnob.Values);
		OnPropertyChanged("SelectedValueIndex");
		base.RaisePropertyChangedEvents();
	}
}

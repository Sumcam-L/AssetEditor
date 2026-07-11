using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels.KnobViewModels;

public class KnobSetViewModel : Notifier, IDisposable
{
	private string m_knobSetName;

	private string m_defaultKnobSetName;

	private ObservableCollection<KnobViewModel> m_knobs = new ObservableCollection<KnobViewModel>();

	private Dictionary<string, ObservableCollection<KnobViewModel>> m_knobSubGroups = new Dictionary<string, ObservableCollection<KnobViewModel>>();

	private IKnobSet m_knobSet;

	public string DefaultKnobSetName
	{
		get
		{
			return m_defaultKnobSetName;
		}
		set
		{
			m_defaultKnobSetName = value ?? string.Empty;
		}
	}

	public ObservableCollection<KnobViewModel> Knobs
	{
		get
		{
			return m_knobs;
		}
		set
		{
			if (m_knobs != value)
			{
				m_knobs = value;
				OnPropertyChanged("Knobs");
			}
		}
	}

	public IKnobSet KnobSet
	{
		get
		{
			return m_knobSet;
		}
		set
		{
			if (m_knobSet != value)
			{
				m_knobSet = value;
			}
		}
	}

	public string KnobSetDisplayName => ((!string.IsNullOrEmpty(m_knobSetName)) ? m_knobSetName : m_defaultKnobSetName) + " Knobs";

	public string KnobSetName
	{
		get
		{
			return m_knobSetName;
		}
		set
		{
			if (m_knobSetName != value)
			{
				m_knobSetName = value;
				OnPropertyChanged("KnobSetName");
				OnPropertyChanged("KnobSetDisplayName");
			}
		}
	}

	public Dictionary<string, ObservableCollection<KnobViewModel>> KnobSubgroups
	{
		get
		{
			return m_knobSubGroups;
		}
		set
		{
			if (m_knobSubGroups != value)
			{
				m_knobSubGroups = value;
				OnPropertyChanged("KnobSubgroups");
			}
		}
	}

	public KnobSetViewModel()
		: this(null)
	{
	}

	public KnobSetViewModel(IKnobSet knobSet)
	{
		if (knobSet != null)
		{
			AddKnobSet(knobSet);
			KnobSetName = knobSet.KnobSetName;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void AddKnobSet(IKnobSet knobSet)
	{
		Dictionary<string, ObservableCollection<KnobViewModel>> dictionary = new Dictionary<string, ObservableCollection<KnobViewModel>>();
		foreach (KeyValuePair<string, IKnobSubgroup> item in knobSet.KnobsBySubgroup)
		{
			if (string.IsNullOrEmpty(item.Key))
			{
				Knobs = ConstructKnobVMs(item.Value.Knobs);
			}
			else
			{
				dictionary[item.Key] = ConstructKnobVMs(item.Value.Knobs);
			}
		}
		KnobSubgroups = dictionary;
		KnobSet = knobSet;
		KnobSetName = knobSet.KnobSetName;
	}

	public void ClearKnobs()
	{
		foreach (KnobViewModel knob in Knobs)
		{
			knob.Dispose();
		}
		foreach (ObservableCollection<KnobViewModel> value in KnobSubgroups.Values)
		{
			foreach (KnobViewModel item in value)
			{
				item.Dispose();
			}
			value.Clear();
		}
		Knobs.Clear();
		KnobSubgroups.Clear();
		KnobSet = null;
	}

	public KnobViewModel ConstructKnobVM(IKnob knob)
	{
		KnobViewModel result = null;
		switch (knob.KnobType)
		{
		case KnobType.KT_FUNCTION:
			result = new FunctionKnobViewModel(knob as IFunctionKnob);
			break;
		case KnobType.KT_VALUE_BOOL:
			result = new ValueKnobViewModel<bool>(knob as IValueKnob<bool>);
			break;
		case KnobType.KT_VALUE_INT:
			result = new ValueKnobViewModel<int>(knob as IValueKnob<int>);
			break;
		case KnobType.KT_VALUE_FLOAT:
			result = new ValueKnobViewModel<float>(knob as IValueKnob<float>);
			break;
		case KnobType.KT_VALUE_STRING:
			result = new ValueKnobViewModel<string>(knob as IValueKnob<string>);
			break;
		case KnobType.KT_VALUE_COLOR:
			result = new ColorValueKnobViewModel(knob as IColorValueKnob);
			break;
		case KnobType.KT_RANGE_FLOAT:
			result = new RangeKnobViewModel<float>(knob as IRangeKnob<float>);
			break;
		case KnobType.KT_RANGE_INT:
			result = new RangeKnobViewModel<int>(knob as IRangeKnob<int>);
			break;
		case KnobType.KT_CONTAINER_FLOAT:
			result = new ContainerKnobViewModel<float>(knob as IContainerKnob<float>);
			break;
		case KnobType.KT_CONTAINER_INT:
			result = new ContainerKnobViewModel<int>(knob as IContainerKnob<int>);
			break;
		case KnobType.KT_CONTAINER_STRING:
			result = new ContainerKnobViewModel<string>(knob as IContainerKnob<string>);
			break;
		}
		return result;
	}

	public void Reset()
	{
		ClearKnobs();
		KnobSetName = string.Empty;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && Knobs != null)
		{
			ClearKnobs();
			Knobs = null;
			KnobSubgroups = null;
		}
	}

	private ObservableCollection<KnobViewModel> ConstructKnobVMs(IEnumerable<IKnob> knobs)
	{
		ObservableCollection<KnobViewModel> observableCollection = new ObservableCollection<KnobViewModel>();
		foreach (IKnob knob in knobs)
		{
			observableCollection.Add(ConstructKnobVM(knob));
		}
		return observableCollection;
	}
}

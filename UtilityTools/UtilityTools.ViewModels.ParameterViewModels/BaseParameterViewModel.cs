using System;
using System.ComponentModel;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class BaseParameterViewModel : Notifier, IParameterViewModel, INotifyPropertyChanged
{
	private IParameter m_parameter;

	private IValue m_value;

	public string ToolTip
	{
		get
		{
			string description = Description;
			return (!string.IsNullOrEmpty(description)) ? description : null;
		}
	}

	public string Description => Parameter.Description;

	public virtual bool IsImporting
	{
		get
		{
			return false;
		}
		protected set
		{
		}
	}

	public string Name => Value.ParameterName;

	public IParameter Parameter
	{
		get
		{
			return m_parameter;
		}
		set
		{
			if (m_parameter != value)
			{
				m_parameter = value;
				OnPropertyChanged("Parameter");
				OnPropertyChanged("Name");
				OnPropertyChanged("Description");
			}
		}
	}

	public IValue Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (m_value != value)
			{
				m_value = value;
				OnPropertyChanged("Value");
			}
		}
	}

	public event EventHandler<ParameterValueChangedEventArgs> ParameterValueChangedEvent;

	public BaseParameterViewModel(IParameter param, IValue value)
	{
		Parameter = param;
		Value = value;
	}

	protected virtual void OnParameterValueChangedEvent(string paramName, IValue newValue)
	{
		this.ParameterValueChangedEvent?.Invoke(this, new ParameterValueChangedEventArgs(paramName, newValue));
	}
}

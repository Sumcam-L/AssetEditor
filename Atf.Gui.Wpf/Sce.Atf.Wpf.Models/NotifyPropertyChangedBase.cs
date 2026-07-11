using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sce.Atf.Wpf.Models;

[Serializable]
public class NotifyPropertyChangedBase : INotifyPropertyChanged
{
	[NonSerialized]
	private PropertyChangedEventHandler m_propertyChanged;

	public event PropertyChangedEventHandler PropertyChanged
	{
		add
		{
			m_propertyChanged = (PropertyChangedEventHandler)Delegate.Combine(m_propertyChanged, value);
		}
		remove
		{
			m_propertyChanged = (PropertyChangedEventHandler)Delegate.Remove(m_propertyChanged, value);
		}
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		m_propertyChanged?.Invoke(this, e);
	}

	[Conditional("DEBUG")]
	private void CheckPropertyName(string propertyName)
	{
		PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(this)[propertyName];
		if (propertyDescriptor == null)
		{
			throw new InvalidOperationException(string.Format(null, "The property with the propertyName '{0}' doesn't exist.", new object[1] { propertyName }));
		}
	}
}

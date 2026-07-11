using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Sce.Atf.Collections;

public class ChildChangeListener : ChangeListener
{
	protected static readonly Type m_inotifyType = typeof(INotifyPropertyChanged);

	private readonly INotifyPropertyChanged m_value;

	private readonly Type m_type;

	private readonly Dictionary<string, ChangeListener> m_childListeners = new Dictionary<string, ChangeListener>();

	public ChildChangeListener(INotifyPropertyChanged instance)
	{
		Requires.NotNull(instance, "instance");
		m_value = instance;
		m_type = m_value.GetType();
		Subscribe();
	}

	public ChildChangeListener(INotifyPropertyChanged instance, string propertyName)
		: this(instance)
	{
		PropertyName = propertyName;
	}

	private void Subscribe()
	{
		m_value.PropertyChanged += ValuePropertyChanged;
		IEnumerable<PropertyInfo> enumerable = from property in m_type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			where m_inotifyType.IsAssignableFrom(property.PropertyType)
			select property;
		foreach (PropertyInfo item in enumerable)
		{
			m_childListeners.Add(item.Name, null);
			ResetChildListener(item.Name);
		}
	}

	private void ResetChildListener(string propertyName)
	{
		if (!m_childListeners.ContainsKey(propertyName))
		{
			return;
		}
		if (m_childListeners[propertyName] != null)
		{
			m_childListeners[propertyName].PropertyChanged -= ChildPropertyChanged;
			m_childListeners[propertyName].Dispose();
			m_childListeners[propertyName] = null;
		}
		PropertyInfo property = m_type.GetProperty(propertyName);
		if (property == null)
		{
			throw new InvalidOperationException($"Was unable to get '{propertyName}' property information from Type '{m_type.Name}'");
		}
		object value = property.GetValue(m_value, null);
		if (value != null)
		{
			if (value is INotifyCollectionChanged)
			{
				m_childListeners[propertyName] = new CollectionChangeListener(value as INotifyCollectionChanged, propertyName);
			}
			else if (value is INotifyPropertyChanged)
			{
				m_childListeners[propertyName] = new ChildChangeListener(value as INotifyPropertyChanged, propertyName);
			}
			if (m_childListeners[propertyName] != null)
			{
				m_childListeners[propertyName].PropertyChanged += ChildPropertyChanged;
			}
		}
	}

	private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		RaisePropertyChanged(sender, e.PropertyName);
	}

	private void ValuePropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		ResetChildListener(e.PropertyName);
		RaisePropertyChanged(sender, e.PropertyName);
	}

	protected override void RaisePropertyChanged(object sender, string propertyName)
	{
		base.RaisePropertyChanged(sender, string.Format("{0}{1}{2}", PropertyName, (PropertyName != null) ? "." : null, propertyName));
	}

	protected override void Unsubscribe()
	{
		m_value.PropertyChanged -= ValuePropertyChanged;
		foreach (string key in m_childListeners.Keys)
		{
			if (m_childListeners[key] != null)
			{
				m_childListeners[key].Dispose();
			}
		}
		m_childListeners.Clear();
	}
}

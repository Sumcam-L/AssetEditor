using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class PropertyNode : NotifyPropertyChangedBase, IDataErrorInfo, IDisposable, IComparable<PropertyNode>, IComparable, IWeakEventListener
{
	private static readonly PropertyChangedEventArgs s_isExpandedArgs = ObservableUtil.CreateArgs((PropertyNode x) => x.IsExpanded);

	private static readonly PropertyChangedEventArgs s_isSelectedArgs = ObservableUtil.CreateArgs((PropertyNode x) => x.IsSelected);

	private static readonly PropertyChangedEventArgs s_readOnlyArgs = ObservableUtil.CreateArgs((PropertyNode x) => x.IsReadOnly);

	private static readonly PropertyChangedEventArgs s_isWriteableArgs = ObservableUtil.CreateArgs((PropertyNode x) => x.IsWriteable);

	private PropertyDescriptor m_descriptor;

	private object m_instance;

	private bool m_isEnumerable;

	private bool m_synchronizing;

	private bool m_disposed;

	private bool m_isExpanded = true;

	private bool m_isSelected;

	private bool m_overrideReadOnly;

	public object Instance => (m_instance is ICustomTypeDescriptor customTypeDescriptor) ? customTypeDescriptor.GetPropertyOwner(m_descriptor) : m_instance;

	public bool IsMultipleInstance
	{
		get
		{
			if (IsEnumerable)
			{
				int num = 0;
				foreach (object item in ((IEnumerable)Instance).Cast<object>())
				{
					if (num++ > 0)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public object FirstInstance => IsEnumerable ? Instances.Cast<object>().FirstOrDefault() : Instance;

	public IEnumerable Instances
	{
		get
		{
			if (IsEnumerable)
			{
				foreach (object instance in (IEnumerable)Instance)
				{
					yield return (instance is ICustomTypeDescriptor customTypeDescriptor) ? customTypeDescriptor.GetPropertyOwner(m_descriptor) : instance;
				}
			}
			else
			{
				yield return Instance;
			}
		}
	}

	public PropertyDescriptor Descriptor => m_descriptor;

	public object Value
	{
		get
		{
			return IsEnumerable ? GetValueFromEnumerable() : GetValue(Instance);
		}
		set
		{
			if (!m_synchronizing)
			{
				try
				{
					m_synchronizing = true;
					SetValue(value);
				}
				finally
				{
					m_synchronizing = false;
				}
			}
		}
	}

	public object OldValue { get; private set; }

	public bool IsEnumerable => m_isEnumerable;

	public bool CanResetValue => Instances.Cast<object>().All((object x) => m_descriptor.CanResetValue(x));

	public object EditorContext { get; set; }

	public object[] StandardValues
	{
		get
		{
			object[] array = null;
			if (!IsEnumerable)
			{
				array = GetStandardValues(FirstInstance);
			}
			else
			{
				foreach (object instance in Instances)
				{
					object[] standardValues = GetStandardValues(instance);
					if (array == null)
					{
						array = standardValues;
					}
					else if (standardValues == null || !array.SequenceEqual(standardValues))
					{
						array = null;
						break;
					}
				}
			}
			return array;
		}
	}

	public bool IsExpanded
	{
		get
		{
			return m_isExpanded;
		}
		set
		{
			m_isExpanded = value;
			OnPropertyChanged(s_isExpandedArgs);
		}
	}

	public bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			m_isSelected = value;
			OnPropertyChanged(s_isSelectedArgs);
		}
	}

	public Exception PropertyValueError { get; private set; }

	string IDataErrorInfo.this[string columnName]
	{
		get
		{
			if (Instance is IDataErrorInfo dataErrorInfo)
			{
				return dataErrorInfo[Descriptor.Name];
			}
			return null;
		}
	}

	string IDataErrorInfo.Error
	{
		get
		{
			if (!(Instance is IDataErrorInfo { Error: var error }))
			{
				return null;
			}
			return error;
		}
	}

	public string Name => Descriptor.Name;

	public bool IsWriteable => !IsReadOnly;

	public virtual bool IsReadOnly
	{
		get
		{
			return Descriptor.IsReadOnly || m_overrideReadOnly;
		}
		set
		{
			m_overrideReadOnly = value;
			OnReadOnlyStateChanged();
		}
	}

	public Type PropertyType => Descriptor.PropertyType;

	public string PropertyName => Descriptor.Name;

	public string DisplayName => Descriptor.DisplayName;

	public string Category => Descriptor.Category;

	public string Description => Descriptor.Description;

	public event EventHandler ValueSetting;

	public event EventHandler ValueSet;

	public event EventHandler ValueChanged;

	public event EventHandler ValueError;

	public void Initialize(object instance, PropertyDescriptor descriptor, bool isEnumerable)
	{
		Requires.NotNull(instance, "instance");
		Requires.NotNull(descriptor, "descriptor");
		m_instance = instance;
		m_descriptor = descriptor;
		m_isEnumerable = isEnumerable;
		InitializeInternal();
		SubscribeValueChanged();
	}

	protected virtual void InitializeInternal()
	{
	}

	public virtual void ResetValue()
	{
		foreach (object instance in Instances)
		{
			if (m_descriptor.CanResetValue(instance))
			{
				m_descriptor.ResetValue(instance);
			}
		}
	}

	public int CompareTo(object obj)
	{
		if (!(obj is PropertyNode other))
		{
			return 0;
		}
		return CompareTo(other);
	}

	public int CompareTo(PropertyNode other)
	{
		IComparable comparable = Value as IComparable;
		IComparable comparable2 = other.Value as IComparable;
		if (comparable != null)
		{
			return (comparable2 == null) ? 1 : comparable.CompareTo(comparable2);
		}
		return (comparable2 != null) ? (-1) : 0;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public ValueEditor GetCustomEditor()
	{
		return m_descriptor.GetEditor(typeof(ValueEditor)) as ValueEditor;
	}

	public virtual void UnBind()
	{
		UnsubscribeValueChanged();
	}

	public virtual void Refresh()
	{
		OnPropertyChanged(ObservableUtil.AllChangedEventArgs);
	}

	protected virtual void SetValue(object value)
	{
		PropertyValueError = null;
		OldValue = Value;
		OnValueSetting();
		try
		{
			foreach (object instance in Instances)
			{
				PropertyUtils.SetProperty(instance, m_descriptor, value);
			}
		}
		catch (Exception propertyValueError)
		{
			PropertyValueError = propertyValueError;
			OnValueError();
			throw;
		}
		OnValueSet();
	}

	protected virtual object GetValue(object instance)
	{
		return m_descriptor.GetValue(instance);
	}

	private object GetValueFromEnumerable()
	{
		object obj = null;
		foreach (object instance in Instances)
		{
			object value = GetValue(instance);
			if (obj == null)
			{
				obj = value;
			}
			if (obj != null && value == null)
			{
				return null;
			}
			if (value != null && !value.Equals(obj))
			{
				return null;
			}
		}
		return obj;
	}

	private object[] GetStandardValues(object instance)
	{
		if (m_descriptor.Converter != null)
		{
			TypeDescriptorContext context = new TypeDescriptorContext(instance, m_descriptor, null);
			if (m_descriptor.Converter.GetStandardValuesExclusive(context))
			{
				TypeConverter.StandardValuesCollection standardValues = m_descriptor.Converter.GetStandardValues(context);
				if (standardValues != null)
				{
					return standardValues.Cast<object>().ToArray();
				}
			}
		}
		return null;
	}

	private void SubscribeValueChanged()
	{
		foreach (object instance in Instances)
		{
			SubscribeValueChanged(instance);
		}
	}

	protected virtual void SubscribeValueChanged(object instance)
	{
		ValueChangedEventManager.AddListener(instance, this, m_descriptor);
	}

	private void UnsubscribeValueChanged()
	{
		foreach (object instance in Instances)
		{
			UnsubscribeValueChanged(instance);
		}
	}

	protected virtual void UnsubscribeValueChanged(object instance)
	{
		ValueChangedEventManager.RemoveListener(instance, this, m_descriptor);
	}

	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return OnReceiveWeakEvent(managerType, sender, e);
	}

	protected virtual bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		if (managerType == typeof(ValueChangedEventManager))
		{
			ValueChangedEventArgs e2 = (ValueChangedEventArgs)e;
			if (e2.PropertyDescriptor == m_descriptor)
			{
				OnInstancePropertyValueChanged();
				return true;
			}
		}
		return false;
	}

	private void OnInstancePropertyValueChanged()
	{
		OnValueChanged();
		Refresh();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			if (disposing)
			{
				UnBind();
				IDisposable disposable = EditorContext as IDisposable;
				EditorContext = null;
				disposable?.Dispose();
				m_descriptor = null;
				m_instance = null;
				this.ValueSetting = null;
				this.ValueSet = null;
				this.ValueChanged = null;
				this.ValueError = null;
			}
			m_disposed = true;
		}
	}

	protected virtual void OnValueSetting()
	{
		this.ValueSetting.Raise(this, EventArgs.Empty);
	}

	protected virtual void OnValueSet()
	{
		this.ValueSet.Raise(this, EventArgs.Empty);
	}

	protected virtual void OnValueError()
	{
		this.ValueError.Raise(this, EventArgs.Empty);
	}

	protected virtual void OnValueChanged()
	{
		this.ValueChanged.Raise(this, EventArgs.Empty);
	}

	protected virtual void OnReadOnlyStateChanged()
	{
		OnPropertyChanged(s_readOnlyArgs);
		OnPropertyChanged(s_isWriteableArgs);
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class FilteringContext : IPropertyEditingContext, IAdaptable, IEquatable<IPropertyEditingContext>
{
	private IPropertyEditingContext _propertyEditingContext;

	private PropertyDescriptor _filterProperty;

	private string _filterValue;

	public PropertyDescriptor FilterProperty
	{
		get
		{
			return _filterProperty;
		}
		set
		{
			if (_filterProperty != value)
			{
				_filterProperty = value;
				if (!string.IsNullOrEmpty(FilterValue))
				{
					OnFilterChanged();
				}
			}
		}
	}

	public string FilterValue
	{
		get
		{
			return _filterValue;
		}
		set
		{
			value = value.ToLower();
			if (_filterValue != value)
			{
				_filterValue = value;
				OnFilterChanged();
			}
		}
	}

	public IEnumerable<object> Items
	{
		get
		{
			if (string.IsNullOrEmpty(FilterValue))
			{
				return _propertyEditingContext.Items;
			}
			List<object> list = new List<object>();
			foreach (object item in _propertyEditingContext.Items)
			{
				bool flag = false;
				if (FilterProperty != null)
				{
					flag = ShouldAddObject(item, FilterProperty, FilterValue);
				}
				else
				{
					foreach (PropertyDescriptor propertyDescriptor in _propertyEditingContext.PropertyDescriptors)
					{
						flag = ShouldAddObject(item, propertyDescriptor, FilterValue);
						if (flag)
						{
							break;
						}
					}
				}
				if (flag)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public IEnumerable<PropertyDescriptor> PropertyDescriptors => _propertyEditingContext.PropertyDescriptors;

	public event EventHandler FilterChanged;

	protected virtual void OnFilterChanged()
	{
		this.FilterChanged?.Invoke(this, EventArgs.Empty);
	}

	public FilteringContext(IPropertyEditingContext editingContext)
	{
		_propertyEditingContext = editingContext;
	}

	private bool ShouldAddObject(object obj, PropertyDescriptor property, string filter)
	{
		bool result = false;
		string propertyText = PropertyUtils.GetPropertyText(obj, property);
		if (!string.IsNullOrEmpty(propertyText))
		{
			propertyText = propertyText.ToLower();
			result = propertyText.Contains(filter);
		}
		return result;
	}

	object IAdaptable.GetAdapter(Type type)
	{
		return _propertyEditingContext.As(type);
	}

	bool IEquatable<IPropertyEditingContext>.Equals(IPropertyEditingContext other)
	{
		return _propertyEditingContext == other;
	}
}

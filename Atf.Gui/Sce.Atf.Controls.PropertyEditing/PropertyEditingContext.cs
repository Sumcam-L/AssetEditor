using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyEditingContext : IPropertyEditingContext
{
	private readonly object[] m_selection;

	private PropertyDescriptor[] m_propertyDescriptors;

	private readonly PropertyEditingContext m_innerContext;

	public object[] Selection => m_selection;

	public object LastSelected => m_selection[m_selection.Length - 1];

	IEnumerable<object> IPropertyEditingContext.Items => m_selection;

	IEnumerable<PropertyDescriptor> IPropertyEditingContext.PropertyDescriptors => GetPropertyDescriptors(this);

	public PropertyEditingContext(object[] selection)
		: this(selection, null)
	{
	}

	public PropertyEditingContext(object[] selection, PropertyEditingContext innerContext)
	{
		m_selection = selection;
		m_innerContext = innerContext;
	}

	public virtual bool AreEqual(object value1, object value2)
	{
		return PropertyUtils.AreEqual(value1, value2);
	}

	public virtual PropertyDescriptor[] GetPropertyDescriptors(object owner)
	{
		if (m_innerContext != null)
		{
			return m_innerContext.GetPropertyDescriptors(owner);
		}
		return PropertyUtils.GetDefaultProperties2(owner);
	}

	public static PropertyDescriptor[] GetPropertyDescriptors(PropertyEditingContext context)
	{
		if (context == null || context.m_selection.Length == 0)
		{
			return new PropertyDescriptor[0];
		}
		if (context.m_propertyDescriptors != null)
		{
			return context.m_propertyDescriptors;
		}
		return context.m_propertyDescriptors = context.CreatePropertyDescriptors();
	}

	public bool CanResetValue(PropertyDescriptor descriptor)
	{
		object[] selection = m_selection;
		foreach (object component in selection)
		{
			if (!descriptor.CanResetValue(component))
			{
				return false;
			}
		}
		return true;
	}

	public void ResetValue(PropertyDescriptor descriptor)
	{
		object[] selection = m_selection;
		foreach (object component in selection)
		{
			descriptor.ResetValue(component);
		}
	}

	protected virtual PropertyDescriptor[] CreatePropertyDescriptors()
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>(GetPropertyDescriptors(m_selection[0]));
		for (int i = 1; i < m_selection.Length; i++)
		{
			HashSet<PropertyDescriptor> hashSet = new HashSet<PropertyDescriptor>(GetPropertyDescriptors(m_selection[i]));
			int num = 0;
			while (num < list.Count)
			{
				if (!hashSet.Contains(list[num]))
				{
					list.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
		return list.ToArray();
	}
}

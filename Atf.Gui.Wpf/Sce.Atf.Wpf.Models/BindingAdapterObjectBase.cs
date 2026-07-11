using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Models;

internal abstract class BindingAdapterObjectBase : CustomTypeDescriptor, IAdaptable, INotifyPropertyChanged
{
	private PropertyDescriptorCollection m_cachedPropertyDescriptors;

	public object Adaptee { get; private set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public BindingAdapterObjectBase(object adaptee)
	{
		Adaptee = adaptee;
	}

	public override PropertyDescriptorCollection GetProperties()
	{
		return GetProperties(null);
	}

	public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		if (m_cachedPropertyDescriptors == null && Adaptee != null)
		{
			m_cachedPropertyDescriptors = GenerateDescriptors();
		}
		return m_cachedPropertyDescriptors;
	}

	public object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(GetType()))
		{
			return this;
		}
		return Adaptee.As(type);
	}

	protected abstract PropertyDescriptorCollection GenerateDescriptors();

	protected static void MergeDescriptors(List<PropertyDescriptor> result, PropertyDescriptor[] descriptors)
	{
		foreach (PropertyDescriptor descriptor in descriptors)
		{
			if (!result.Any((PropertyDescriptor x) => x.Name == descriptor.Name))
			{
				result.Add(descriptor);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf.Adaptation;

public abstract class BindingAdapterObjectBase : CustomTypeDescriptor, IAdaptable, INotifyPropertyChanged
{
	private PropertyDescriptorCollection m_cachedPropertyDescriptors;

	public object Adaptee { get; private set; }

	public event PropertyChangedEventHandler PropertyChanged;

	protected BindingAdapterObjectBase(object adaptee)
	{
		Adaptee = adaptee;
		if (this.PropertyChanged == null)
		{
		}
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
		return type.IsAssignableFrom(GetType()) ? this : Adaptee.As(type);
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

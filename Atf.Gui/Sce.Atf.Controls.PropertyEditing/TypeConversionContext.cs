using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class TypeConversionContext : ITypeDescriptorContext, IServiceProvider, IDisposable
{
	private object m_data;

	private PropertyDescriptor m_descriptor;

	public IContainer Container => null;

	public object Instance => m_data;

	public PropertyDescriptor PropertyDescriptor => m_descriptor;

	public TypeConversionContext(object data, PropertyDescriptor desc)
	{
		m_data = data;
		m_descriptor = desc;
	}

	public void Dispose()
	{
		m_data = null;
		m_descriptor = null;
	}

	public object GetService(Type serviceType)
	{
		if (typeof(ITypeDescriptorContext).IsAssignableFrom(serviceType))
		{
			return this;
		}
		return null;
	}

	public void OnComponentChanged()
	{
	}

	public bool OnComponentChanging()
	{
		return true;
	}
}

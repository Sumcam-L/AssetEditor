using System;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class TypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
{
	private readonly object m_owner;

	private readonly PropertyDescriptor m_descriptor;

	private readonly IServiceProvider m_next;

	public IContainer Container => null;

	public object Instance => (m_owner is ICustomTypeDescriptor customTypeDescriptor) ? customTypeDescriptor.GetPropertyOwner(m_descriptor) : m_owner;

	public PropertyDescriptor PropertyDescriptor => m_descriptor;

	public TypeDescriptorContext(object owner, PropertyDescriptor descriptor, IServiceProvider next)
	{
		m_owner = owner;
		m_descriptor = descriptor;
		m_next = next;
	}

	public object GetService(Type serviceType)
	{
		if (typeof(IWindowsFormsEditorService).IsAssignableFrom(serviceType) || typeof(ITypeDescriptorContext).IsAssignableFrom(serviceType))
		{
			return this;
		}
		if (m_next != null)
		{
			return m_next.GetService(serviceType);
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

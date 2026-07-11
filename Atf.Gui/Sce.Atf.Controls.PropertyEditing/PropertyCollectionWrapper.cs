using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyCollectionWrapper : CustomTypeDescriptor, ICustomTypeDescriptor
{
	private readonly PropertyDescriptorCollection m_properties;

	private readonly object m_propertyOwner;

	public PropertyCollectionWrapper(PropertyDescriptor[] properties)
		: this(properties, null)
	{
	}

	public PropertyCollectionWrapper(PropertyDescriptorCollection properties)
		: this(properties, null)
	{
	}

	public PropertyCollectionWrapper(PropertyDescriptor[] properties, object owner)
		: this(new PropertyDescriptorCollection(properties), owner)
	{
	}

	public PropertyCollectionWrapper(PropertyDescriptorCollection properties, object owner)
	{
		m_properties = properties;
		m_propertyOwner = ((owner != null) ? owner : this);
	}

	public override object GetPropertyOwner(PropertyDescriptor propertyDescriptor)
	{
		return m_propertyOwner;
	}

	public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		return GetProperties();
	}

	public override PropertyDescriptorCollection GetProperties()
	{
		return m_properties;
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf.Dom;

public class ObservableCustomTypeDescriptorNodeAdapter : ObservableDomNodeAdapter, ICustomTypeDescriptor
{
	private PropertyDescriptorCollection m_cachedPropertyDescriptors;

	protected virtual System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
		for (DomNodeType domNodeType = base.DomNode.Type; domNodeType != null; domNodeType = domNodeType.BaseType)
		{
			PropertyDescriptorCollection tag = domNodeType.GetTag<PropertyDescriptorCollection>();
			if (tag != null)
			{
				foreach (System.ComponentModel.PropertyDescriptor item in tag)
				{
					if (!hashSet.Contains(item.Name))
					{
						hashSet.Add(item.Name);
						list.Add(item);
					}
				}
			}
		}
		return list.ToArray();
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return GetPropertyDescriptorCollection();
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return GetPropertyDescriptorCollection();
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return base.DomNode.Type.Name;
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return TypeDescriptor.GetComponentName(this, noCustomTypeDesc: true);
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(this, noCustomTypeDesc: true);
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(this, noCustomTypeDesc: true);
	}

	System.ComponentModel.PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(this, noCustomTypeDesc: true);
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(this, editorBaseType, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(this, attributes, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(this, noCustomTypeDesc: true);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd)
	{
		return base.DomNode;
	}

	private PropertyDescriptorCollection GetPropertyDescriptorCollection()
	{
		if (m_cachedPropertyDescriptors == null)
		{
			System.ComponentModel.PropertyDescriptor[] propertyDescriptors = GetPropertyDescriptors();
			ProcessDescriptors(propertyDescriptors);
			m_cachedPropertyDescriptors = new PropertyDescriptorCollection(propertyDescriptors);
		}
		return m_cachedPropertyDescriptors;
	}

	private void ProcessDescriptors(System.ComponentModel.PropertyDescriptor[] descriptors)
	{
		foreach (AttributePropertyDescriptor item in descriptors.OfType<AttributePropertyDescriptor>())
		{
			PropertyChangedEventArgsCollection propertyChangedEventArgsCollection = item.AttributeInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
			if (propertyChangedEventArgsCollection == null)
			{
				propertyChangedEventArgsCollection = new PropertyChangedEventArgsCollection();
				item.AttributeInfo.SetTag(propertyChangedEventArgsCollection);
			}
			propertyChangedEventArgsCollection.Add(new PropertyChangedEventArgs(item.DisplayName));
		}
	}
}

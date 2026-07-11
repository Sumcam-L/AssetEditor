using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Dom;

public class CustomTypeDescriptorNodeAdapter : DomNodeAdapter, ICustomTypeDescriptor
{
	protected virtual PropertyDescriptor[] GetPropertyDescriptors()
	{
		HashSet<string> hashSet = new HashSet<string>();
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		for (DomNodeType domNodeType = base.DomNode.Type; domNodeType != null; domNodeType = domNodeType.BaseType)
		{
			PropertyDescriptorCollection tag = domNodeType.GetTag<PropertyDescriptorCollection>();
			if (tag != null)
			{
				foreach (PropertyDescriptor item2 in tag)
				{
					string item = $"{item2.Category}_{item2.Name}";
					if (!hashSet.Contains(item))
					{
						hashSet.Add(item);
						list.Add(item2);
					}
				}
			}
		}
		return list.ToArray();
	}

	protected virtual string GetClassName()
	{
		return base.DomNode.Type.Name;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return new PropertyDescriptorCollection(GetPropertyDescriptors());
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return new PropertyDescriptorCollection(GetPropertyDescriptors());
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return GetClassName();
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

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
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

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return base.DomNode;
	}
}

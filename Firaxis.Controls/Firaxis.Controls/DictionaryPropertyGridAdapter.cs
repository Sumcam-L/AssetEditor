using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Firaxis.Controls;

public class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
{
	private IDictionary m_Dictionary;

	private IDictionary m_Descriptions;

	public DictionaryPropertyGridAdapter(IDictionary dictionary)
		: this(dictionary, null)
	{
	}

	public DictionaryPropertyGridAdapter(IDictionary dictionary, IDictionary descriptions)
	{
		m_Dictionary = dictionary;
		m_Descriptions = descriptions;
	}

	public AttributeCollection GetAttributes()
	{
		return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
	}

	public string GetClassName()
	{
		return TypeDescriptor.GetClassName(this, noCustomTypeDesc: true);
	}

	public string GetComponentName()
	{
		return TypeDescriptor.GetComponentName(this, noCustomTypeDesc: true);
	}

	public TypeConverter GetConverter()
	{
		return TypeDescriptor.GetConverter(this, noCustomTypeDesc: true);
	}

	public EventDescriptor GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(this, noCustomTypeDesc: true);
	}

	public PropertyDescriptor GetDefaultProperty()
	{
		return null;
	}

	public object GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(this, editorBaseType, noCustomTypeDesc: true);
	}

	public EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(this, attributes, noCustomTypeDesc: true);
	}

	public EventDescriptorCollection GetEvents()
	{
		return TypeDescriptor.GetEvents(this, noCustomTypeDesc: true);
	}

	public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		if (m_Descriptions != null)
		{
			foreach (DictionaryEntry item in m_Dictionary)
			{
				if (m_Descriptions.Contains(item.Key))
				{
					string text = (string)m_Descriptions[item.Key];
					if (string.IsNullOrEmpty(text))
					{
						list.Add(new DictionaryPropertyDescriptor(m_Dictionary, item.Key));
						continue;
					}
					list.Add(new DictionaryPropertyDescriptor(m_Dictionary, item.Key, new Attribute[1]
					{
						new DescriptionAttribute(text)
					}));
				}
				else
				{
					list.Add(new DictionaryPropertyDescriptor(m_Dictionary, item.Key));
				}
			}
		}
		else
		{
			foreach (DictionaryEntry item2 in m_Dictionary)
			{
				list.Add(new DictionaryPropertyDescriptor(m_Dictionary, item2.Key));
			}
		}
		return new PropertyDescriptorCollection(list.ToArray());
	}

	public PropertyDescriptorCollection GetProperties()
	{
		return GetProperties(new Attribute[0]);
	}

	public object GetPropertyOwner(PropertyDescriptor pd)
	{
		return m_Dictionary;
	}
}

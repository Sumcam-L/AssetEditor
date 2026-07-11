using System;
using System.Collections;
using System.ComponentModel;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class ChildAttributeCollectionPropertyDescriptor : PropertyDescriptor
{
	private readonly AttributeInfo[] m_attributeInfos;

	private readonly ChildInfo[] m_childInfos;

	private readonly object[] m_defaultValues;

	public ChildAttributeCollectionPropertyDescriptor(string name, AttributeInfo[] attributeInfos, ChildInfo childInfo, string category, string description, bool isReadOnly)
		: this(name, attributeInfos, childInfo, category, description, isReadOnly, null, null)
	{
	}

	public ChildAttributeCollectionPropertyDescriptor(string name, AttributeInfo[] attributeInfos, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, attributeInfos, childInfo, category, description, isReadOnly, editor, null)
	{
	}

	public ChildAttributeCollectionPropertyDescriptor(string name, AttributeInfo[] attributeInfos, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, attributeInfos, childInfo, category, description, isReadOnly, editor, typeConverter, null)
	{
	}

	public ChildAttributeCollectionPropertyDescriptor(string name, AttributeInfo[] attributeInfos, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, object[] defaultValues)
		: base(name, typeof(IList), category, description, isReadOnly, editor, typeConverter)
	{
		m_attributeInfos = attributeInfos;
		m_childInfos = new ChildInfo[1] { childInfo };
		m_defaultValues = defaultValues;
	}

	public ChildAttributeCollectionPropertyDescriptor(string name, AttributeInfo[] attributeInfos, ChildInfo[] childInfos, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, attributeInfos, childInfos, category, description, isReadOnly, editor, typeConverter, null)
	{
	}

	public ChildAttributeCollectionPropertyDescriptor(string name, AttributeInfo[] attributeInfos, ChildInfo[] childInfos, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, object[] defaultValues)
		: base(name, typeof(IList), category, description, isReadOnly, editor, typeConverter)
	{
		m_attributeInfos = attributeInfos;
		m_childInfos = childInfos;
		m_defaultValues = defaultValues;
	}

	public override bool Equals(object obj)
	{
		ChildAttributeCollectionPropertyDescriptor childAttributeCollectionPropertyDescriptor = obj as ChildAttributeCollectionPropertyDescriptor;
		if (!base.Equals((object)childAttributeCollectionPropertyDescriptor))
		{
			return false;
		}
		if (m_attributeInfos.Length != childAttributeCollectionPropertyDescriptor.m_attributeInfos.Length)
		{
			return false;
		}
		for (int i = 0; i < m_attributeInfos.Length; i++)
		{
			if (m_attributeInfos[i] != childAttributeCollectionPropertyDescriptor.m_attributeInfos[i])
			{
				return false;
			}
		}
		for (int j = 0; j < m_childInfos.Length; j++)
		{
			if (m_childInfos[j] != childAttributeCollectionPropertyDescriptor.m_childInfos[j])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		AttributeInfo[] attributeInfos = m_attributeInfos;
		foreach (AttributeInfo attributeInfo in attributeInfos)
		{
			num ^= attributeInfo.GetHashCode();
		}
		return num;
	}

	public override DomNode GetNode(object component)
	{
		DomNode domNode = component.As<DomNode>();
		if (domNode != null)
		{
			ChildInfo[] childInfos = m_childInfos;
			foreach (ChildInfo childInfo in childInfos)
			{
				ChildInfo childInfo2 = domNode.Type.GetChildInfo(childInfo.Name);
				if (childInfo2 != null)
				{
					return domNode.GetChild(childInfo2);
				}
			}
		}
		return domNode;
	}

	public override bool CanResetValue(object component)
	{
		if (IsReadOnly)
		{
			return false;
		}
		DomNode node = GetNode(component);
		if (node != null)
		{
			for (int i = 0; i < m_attributeInfos.Length; i++)
			{
				AttributeInfo attributeInfo = m_attributeInfos[i];
				object attribute = node.GetAttribute(attributeInfo);
				if (m_defaultValues != null && m_defaultValues.Length > i)
				{
					if (attribute != m_defaultValues[i])
					{
						return true;
					}
				}
				else if (attribute != attributeInfo.DefaultValue)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void ResetValue(object component)
	{
		DomNode node = GetNode(component);
		if (node == null)
		{
			return;
		}
		for (int i = 0; i < m_attributeInfos.Length; i++)
		{
			AttributeInfo attributeInfo = m_attributeInfos[i];
			object attribute = node.GetAttribute(attributeInfo);
			if (m_defaultValues != null && m_defaultValues.Length > i)
			{
				node.SetAttribute(attributeInfo, m_defaultValues[i]);
			}
			else if (attribute != attributeInfo.DefaultValue)
			{
				node.SetAttribute(attributeInfo, attributeInfo.DefaultValue);
			}
		}
	}

	public override object GetValue(object component)
	{
		object[] array = new object[m_attributeInfos.Length];
		DomNode node = GetNode(component);
		if (node != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = node.GetAttribute(m_attributeInfos[i]);
			}
		}
		return array;
	}

	public override void SetValue(object component, object value)
	{
		DomNode node = GetNode(component);
		if (node == null)
		{
			throw new InvalidOperationException("Attempted to set value of an invalid or null object.");
		}
		object[] array = value as object[];
		if (array.Length != m_attributeInfos.Length)
		{
			throw new InvalidOperationException("Array of values has incorrect dimension.");
		}
		for (int i = 0; i < m_attributeInfos.Length; i++)
		{
			node.SetAttribute(m_attributeInfos[i], ((Array)value).GetValue(i));
		}
	}
}

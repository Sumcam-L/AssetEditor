using System;
using System.ComponentModel;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class AttributePropertyDescriptor : PropertyDescriptor
{
	private readonly AttributeInfo m_attributeInfo;

	public AttributeInfo AttributeInfo => m_attributeInfo;

	public AttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly)
		: this(name, attribute, category, description, isReadOnly, null, null)
	{
	}

	public AttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor)
		: this(name, attribute, category, description, isReadOnly, editor, null)
	{
	}

	public AttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, attribute, category, description, isReadOnly, editor, typeConverter, null)
	{
	}

	public AttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, Attribute[] attributes)
		: base(name, attribute.Type.ClrType, category, description, isReadOnly, editor, typeConverter, attributes)
	{
		m_attributeInfo = attribute;
	}

	public override bool ShouldSerializeValue(object component)
	{
		object value = GetValue(component);
		if (value == null)
		{
			return false;
		}
		return !value.Equals(m_attributeInfo.DefaultValue);
	}

	public override bool CanResetValue(object component)
	{
		if (IsReadOnly)
		{
			return false;
		}
		DomNode node = GetNode(component);
		if (node != null && m_attributeInfo.Equivalent(node.Type.IdAttribute))
		{
			return false;
		}
		object value = GetValue(component);
		return (value != null && !value.Equals(m_attributeInfo.DefaultValue)) || (value == null && m_attributeInfo.DefaultValue != null);
	}

	public override void ResetValue(object component)
	{
		SetValue(component, m_attributeInfo.DefaultValue);
	}

	public override object GetValue(object component)
	{
		object result = null;
		DomNode node = GetNode(component);
		if (node != null)
		{
			result = node.GetAttribute(m_attributeInfo);
		}
		return result;
	}

	public override void SetValue(object component, object value)
	{
		GetNode(component)?.SetAttribute(m_attributeInfo, value);
	}

	public override bool Equals(object obj)
	{
		return obj is AttributePropertyDescriptor attributePropertyDescriptor && m_attributeInfo.Equivalent(attributePropertyDescriptor.m_attributeInfo);
	}

	public override int GetHashCode()
	{
		return m_attributeInfo.GetEquivalentHashCode();
	}

	public override DomNode GetNode(object component)
	{
		DomNode domNode = component.As<DomNode>();
		if (domNode != null && !domNode.Type.IsValid(m_attributeInfo))
		{
			return null;
		}
		return domNode;
	}
}

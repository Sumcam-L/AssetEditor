using System;
using System.ComponentModel;

namespace Sce.Atf.Dom;

public class DomNodePropertyMatch : IQueryMatch
{
	protected PropertyDescriptor m_propertyDescriptor;

	protected DomNode m_domNode;

	public string Name
	{
		get
		{
			return m_propertyDescriptor.Name;
		}
		set
		{
		}
	}

	public PropertyDescriptor PropertyDescriptor => m_propertyDescriptor;

	private DomNodePropertyMatch()
	{
	}

	public DomNodePropertyMatch(PropertyDescriptor property, DomNode domNode)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property", "Cannot create a DomNodePropertyMatchCandiate with a null property.");
		}
		if (domNode == null)
		{
			throw new ArgumentNullException("domNode", "Cannot create a DomNodePropertyMatchCandiate with a null domNode.");
		}
		m_propertyDescriptor = property;
		m_domNode = domNode;
	}

	public object GetValue()
	{
		return m_propertyDescriptor.GetValue(m_domNode);
	}

	public void SetValue(object value)
	{
		object obj = Convert.ChangeType(value, GetValue().GetType());
		if (obj == null)
		{
			throw new InvalidOperationException("Attempted to replace the value of a search result with an incompatible type");
		}
		m_propertyDescriptor.SetValue(m_domNode, obj);
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public abstract class DomNodeAdapter : IAdapter, IAdaptable, IDecoratable
{
	private DomNode m_domNode;

	[Browsable(false)]
	public DomNode DomNode
	{
		get
		{
			return m_domNode;
		}
		internal set
		{
			m_domNode = value;
			OnNodeSet();
		}
	}

	object IAdapter.Adaptee
	{
		get
		{
			return m_domNode;
		}
		set
		{
			if (m_domNode != null)
			{
				throw new InvalidOperationException("DomNode already set");
			}
			if (!(value is DomNode domNode))
			{
				throw new InvalidOperationException("value must be DomNode");
			}
			DomNode = domNode;
		}
	}

	public virtual IEnumerable<object> GetDecorators(Type type)
	{
		return m_domNode.GetDecorators(type);
	}

	public virtual object GetAdapter(Type type)
	{
		return m_domNode.GetAdapter(type);
	}

	protected virtual void OnNodeSet()
	{
	}

	internal void ParentNodeSetInternal()
	{
		OnParentNodeSet();
	}

	protected virtual void OnParentNodeSet()
	{
	}

	protected T GetParentAs<T>() where T : class
	{
		DomNode parent = DomNode.Parent;
		return (parent != null) ? parent.As<T>() : null;
	}

	protected T GetAttribute<T>(AttributeInfo attributeInfo)
	{
		object attribute = DomNode.GetAttribute(attributeInfo);
		if (attribute != null)
		{
			return (T)attribute;
		}
		return default(T);
	}

	protected void SetAttribute(AttributeInfo attributeInfo, object value)
	{
		DomNode.SetAttribute(attributeInfo, value);
	}

	protected T GetReference<T>(AttributeInfo attributeInfo) where T : class
	{
		if (DomNode.GetAttribute(attributeInfo) is DomNode adaptable)
		{
			return adaptable.As<T>();
		}
		return null;
	}

	protected void SetReference(AttributeInfo attributeInfo, IAdaptable value)
	{
		DomNode value2 = value.As<DomNode>();
		DomNode.SetAttribute(attributeInfo, value2);
	}

	protected T GetChild<T>(ChildInfo childInfo) where T : class
	{
		DomNode child = DomNode.GetChild(childInfo);
		if (child != null)
		{
			return child.As<T>();
		}
		return null;
	}

	protected void SetChild(ChildInfo childInfo, IAdaptable value)
	{
		DomNode child = value.As<DomNode>();
		DomNode.SetChild(childInfo, child);
	}

	protected IList<T> GetChildList<T>(ChildInfo childInfo) where T : class
	{
		return new DomNodeListAdapter<T>(DomNode, childInfo);
	}

	protected T GetExtension<T>(ExtensionInfo extensionInfo) where T : class
	{
		return DomNode.GetExtension(extensionInfo).As<T>();
	}

	public override bool Equals(object obj)
	{
		if (m_domNode == null)
		{
			throw new InvalidOperationException("node not set");
		}
		return m_domNode.Equals(obj);
	}

	public override int GetHashCode()
	{
		if (m_domNode == null)
		{
			throw new InvalidOperationException("node not set");
		}
		return m_domNode.GetHashCode();
	}

	public override string ToString()
	{
		if (m_domNode != null)
		{
			return base.ToString() + ", on " + m_domNode;
		}
		return base.ToString() + ", DomNode is not set";
	}
}

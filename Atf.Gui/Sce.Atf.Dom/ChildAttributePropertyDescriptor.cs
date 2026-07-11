using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class ChildAttributePropertyDescriptor : AttributePropertyDescriptor
{
	private readonly IEnumerable<ChildInfo> m_childPath;

	private readonly IEnumerable<int> m_childIndices;

	public override IEnumerable<ChildInfo> Path => m_childPath;

	public ChildAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, string category, string description, bool isReadOnly)
		: this(name, attributeInfo, childInfo, category, description, isReadOnly, null, null)
	{
	}

	public ChildAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, attributeInfo, childInfo, category, description, isReadOnly, editor, null)
	{
	}

	public ChildAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_childPath = new ChildInfo[1] { childInfo };
	}

	public ChildAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, int childIndex, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_childPath = new ChildInfo[1] { childInfo };
		m_childIndices = new int[1] { childIndex };
	}

	public ChildAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, IEnumerable<ChildInfo> childPath, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_childPath = childPath;
	}

	public ChildAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, IEnumerable<ChildInfo> childPath, IEnumerable<int> childIndices, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_childPath = childPath;
		m_childIndices = childIndices;
	}

	public override DomNode GetNode(object component)
	{
		DomNode domNode = component.As<DomNode>();
		if (domNode != null)
		{
			bool flag = false;
			IEnumerator<int> enumerator = ((m_childIndices == null) ? null : m_childIndices.GetEnumerator());
			if (enumerator != null)
			{
				flag = enumerator.MoveNext();
			}
			foreach (ChildInfo item in m_childPath)
			{
				if (!domNode.Type.IsValid(item))
				{
					return null;
				}
				if (!flag)
				{
					domNode = domNode.GetChild(item);
				}
				else
				{
					IList<DomNode> childList = domNode.GetChildList(item);
					if (childList.Count <= enumerator.Current)
					{
						return null;
					}
					domNode = childList[enumerator.Current];
					flag = enumerator.MoveNext();
				}
				if (domNode == null)
				{
					return null;
				}
			}
			if (!domNode.Type.IsValid(base.AttributeInfo))
			{
				return null;
			}
		}
		return domNode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ChildAttributePropertyDescriptor childAttributePropertyDescriptor) || !base.Equals((object)childAttributePropertyDescriptor))
		{
			return false;
		}
		IEnumerator<ChildInfo> enumerator = m_childPath.GetEnumerator();
		IEnumerator<ChildInfo> enumerator2 = childAttributePropertyDescriptor.m_childPath.GetEnumerator();
		while (true)
		{
			bool flag = enumerator.MoveNext();
			bool flag2 = enumerator2.MoveNext();
			if (flag != flag2)
			{
				return false;
			}
			if (!flag)
			{
				return true;
			}
			if (!enumerator.Current.Equivalent(enumerator2.Current))
			{
				break;
			}
			bool flag3 = true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		foreach (ChildInfo item in m_childPath)
		{
			num ^= item.GetEquivalentHashCode();
		}
		return num;
	}
}

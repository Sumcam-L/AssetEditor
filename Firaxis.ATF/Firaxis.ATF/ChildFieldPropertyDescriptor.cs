using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class ChildFieldPropertyDescriptor : FieldPropertyDescriptorBase
{
	private readonly IEnumerable<int> m_childIndices;

	private readonly IEnumerable<ChildInfo> m_childPath;

	public override AttributeInfo AttributeInfo => null;

	public override Type ClrType => typeof(object);

	public override IEnumerable<ChildInfo> Path => m_childPath;

	public ChildFieldPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, new ChildInfo[1] { childInfo }, null, category, description, isReadOnly, editor, null)
	{
	}

	public ChildFieldPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter conv)
		: this(name, new ChildInfo[1] { childInfo }, null, category, description, isReadOnly, editor, conv)
	{
	}

	public ChildFieldPropertyDescriptor(string name, ChildInfo childInfo, int childIndex, string category, string description, bool isReadOnly, object editor)
		: this(name, new ChildInfo[1] { childInfo }, new int[1] { childIndex }, category, description, isReadOnly, editor, null)
	{
	}

	public ChildFieldPropertyDescriptor(string name, IEnumerable<ChildInfo> childPath, IEnumerable<int> childIndices, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, null, category, description, isReadOnly, editor, typeConverter)
	{
		m_childPath = childPath;
		m_childIndices = childIndices;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ChildAttributeFieldPropertyDescriptor childAttributeFieldPropertyDescriptor) || !base.Equals((object)childAttributeFieldPropertyDescriptor))
		{
			return false;
		}
		IEnumerator<ChildInfo> enumerator = m_childPath.GetEnumerator();
		IEnumerator<ChildInfo> enumerator2 = childAttributeFieldPropertyDescriptor.Path.GetEnumerator();
		do
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
		}
		while (enumerator.Current.Equivalent(enumerator2.Current));
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
		}
		return domNode;
	}
}

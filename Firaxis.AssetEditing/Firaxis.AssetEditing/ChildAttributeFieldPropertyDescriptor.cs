using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.ATF;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ChildAttributeFieldPropertyDescriptor : FieldPropertyDescriptorBase
{
	private AttributeInfo m_attributeInfo;

	private readonly IEnumerable<ChildInfo> m_childPath;

	private readonly IEnumerable<int> m_childIndices;

	public override AttributeInfo AttributeInfo => m_attributeInfo;

	public override Type ClrType => AttributeInfo.Type.ClrType;

	public override IEnumerable<ChildInfo> Path => m_childPath;

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, attributeInfo, new ChildInfo[1] { childInfo }, null, category, description, isReadOnly, editor, null)
	{
	}

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter conv)
		: this(name, attributeInfo, new ChildInfo[1] { childInfo }, null, category, description, isReadOnly, editor, conv)
	{
	}

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor)
		: this(name, attributeInfo, childInfo, category, description, isReadOnlyFunctor, editor, null)
	{
	}

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter conv)
		: this(name, attributeInfo, new ChildInfo[1] { childInfo }, null, category, description, isReadOnlyFunctor, editor, conv)
	{
	}

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, ChildInfo childInfo, int childIndex, string category, string description, bool isReadOnly, object editor)
		: this(name, attributeInfo, new ChildInfo[1] { childInfo }, new int[1] { childIndex }, category, description, isReadOnly, editor, null)
	{
	}

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, IEnumerable<ChildInfo> childPath, IEnumerable<int> childIndices, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_attributeInfo = attributeInfo;
		m_childPath = childPath;
		m_childIndices = childIndices;
	}

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, IEnumerable<ChildInfo> childPath, IEnumerable<int> childIndices, string category, string description, Func<bool> isReadOnlyFunctor, object editor)
		: this(name, attributeInfo, childPath, childIndices, category, description, isReadOnlyFunctor, editor, null)
	{
	}

	public ChildAttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, IEnumerable<ChildInfo> childPath, IEnumerable<int> childIndices, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnlyFunctor, editor, typeConverter)
	{
		m_attributeInfo = attributeInfo;
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
		IEnumerator<ChildInfo> enumerator2 = childAttributeFieldPropertyDescriptor.m_childPath.GetEnumerator();
		do
		{
			bool flag = enumerator.MoveNext();
			bool flag2 = enumerator2.MoveNext();
			if (flag == flag2)
			{
				if (!flag)
				{
					return true;
				}
				continue;
			}
			return false;
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
			IEnumerator<int> enumerator = m_childIndices?.GetEnumerator();
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
			if (!domNode.Type.IsValid(AttributeInfo))
			{
				return null;
			}
		}
		return domNode;
	}
}

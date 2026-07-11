using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class ChildPropertyDescriptor : PropertyDescriptor
{
	private readonly IList<ChildInfo> m_childInfoPath;

	public override IEnumerable<ChildInfo> Path => m_childInfoPath;

	public ChildInfo ChildInfo => m_childInfoPath[m_childInfoPath.Count - 1];

	public ChildPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, bool isReadOnly)
		: this(name, childInfo, category, description, isReadOnly, null, null)
	{
	}

	public ChildPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, childInfo, category, description, isReadOnly, editor, null)
	{
	}

	public ChildPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, new List<ChildInfo> { childInfo }, category, description, isReadOnly, editor, typeConverter)
	{
	}

	public ChildPropertyDescriptor(string name, IList<ChildInfo> childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, typeof(DomNode), category, description, isReadOnly, editor, typeConverter)
	{
		m_childInfoPath = childInfo;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ChildPropertyDescriptor childPropertyDescriptor))
		{
			return false;
		}
		ILookup<ChildInfo, ChildInfo> lookup = m_childInfoPath.ToLookup((ChildInfo i) => i);
		ILookup<ChildInfo, ChildInfo> list2Groups = childPropertyDescriptor.m_childInfoPath.ToLookup((ChildInfo i) => i);
		return lookup.Count == list2Groups.Count && lookup.All((IGrouping<ChildInfo, ChildInfo> g) => g.Count() == list2Groups[g.Key].Count());
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		foreach (ChildInfo item in m_childInfoPath)
		{
			num ^= item.GetHashCode();
		}
		return num;
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override void ResetValue(object component)
	{
		throw new InvalidOperationException("Can't reset value");
	}

	public override DomNode GetNode(object component)
	{
		DomNode domNode = component.As<DomNode>();
		foreach (ChildInfo item in Path)
		{
			if (!domNode.Type.IsValid(item))
			{
				return null;
			}
			if (item != ChildInfo)
			{
				domNode = domNode.GetChild(item).As<DomNode>();
			}
		}
		return domNode;
	}

	public override object GetValue(object component)
	{
		DomNode node = GetNode(component);
		if (node != null && node.Type.IsValid(ChildInfo))
		{
			if (ChildInfo.IsList)
			{
				return node.GetChildList(ChildInfo);
			}
			return node.GetChild(ChildInfo);
		}
		return null;
	}

	public override void SetValue(object component, object value)
	{
		DomNode node = GetNode(component);
		if (node == null)
		{
			return;
		}
		if (ChildInfo.IsList)
		{
			IList<DomNode> childList = node.GetChildList(ChildInfo);
			IList<DomNode> list = value as IList<DomNode>;
			if (childList != null && list != null)
			{
				int i;
				for (i = 0; i < Math.Min(list.Count, childList.Count) && childList[i] == list[i]; i++)
				{
				}
				while (childList.Count > i)
				{
					childList.RemoveAt(i);
				}
				for (; i < list.Count; i++)
				{
					childList.Add(list[i]);
				}
			}
		}
		else
		{
			DomNode node2 = GetNode(value);
			if (node2 != null)
			{
				node.SetChild(ChildInfo, node2);
			}
		}
	}
}

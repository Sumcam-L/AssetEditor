using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ChildFieldPropertyDescriptor : FieldPropertyDescriptorBase
{
	private readonly IEnumerable<ChildInfo> m_childPath;

	private readonly IEnumerable<int> m_childIndices;

	public override AttributeInfo AttributeInfo => null;

	public override Type ClrType => typeof(object);

	public override IEnumerable<ChildInfo> Path => m_childPath;

	public ChildFieldPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, new ChildInfo[1] { childInfo }, null, category, description, isReadOnly, editor, null)
	{
	}

	public ChildFieldPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor)
		: this(name, new ChildInfo[1] { childInfo }, null, category, description, isReadOnlyFunctor, editor, null)
	{
	}

	public ChildFieldPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, bool isReadOnly, object editor, TypeConverter conv)
		: this(name, new ChildInfo[1] { childInfo }, null, category, description, isReadOnly, editor, conv)
	{
	}

	public ChildFieldPropertyDescriptor(string name, ChildInfo childInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter conv)
		: this(name, new ChildInfo[1] { childInfo }, null, category, description, isReadOnlyFunctor, editor, conv)
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

	public ChildFieldPropertyDescriptor(string name, IEnumerable<ChildInfo> childPath, IEnumerable<int> childIndices, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter)
		: base(name, null, category, description, isReadOnlyFunctor, editor, typeConverter)
	{
		m_childPath = childPath;
		m_childIndices = childIndices;
	}

	public override bool CanResetValue(object component)
	{
		if (IsReadOnly)
		{
			return false;
		}
		DomNode node = GetNode(component);
		if (node != null && AttributeInfo.Equivalent(node.Type.IdAttribute))
		{
			return false;
		}
		object value = GetValue(component);
		if (value == null || value.Equals(AttributeInfo.DefaultValue))
		{
			if (value == null)
			{
				return AttributeInfo.DefaultValue != null;
			}
			return false;
		}
		return true;
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
		return component.As<DomNode>();
	}

	public override object GetValue(object component)
	{
		return GetNode(component).GetChildList(m_childPath.First());
	}

	public override void ResetValue(object component)
	{
		SetValue(component, AttributeInfo.DefaultValue);
	}

	public override void SetValue(object component, object value)
	{
		GetNode(component)?.SetAttribute(AttributeInfo, value);
	}
}

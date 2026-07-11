using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class ChildInfo : FieldMetadata
{
	private readonly DomNodeType m_type;

	private List<ChildRule> m_rules;

	private readonly bool m_isList;

	public DomNodeType Type => m_type;

	public bool IsList => m_isList;

	public IEnumerable<ChildRule> Rules
	{
		get
		{
			if (m_rules != null)
			{
				return m_rules;
			}
			return EmptyEnumerable<ChildRule>.Instance;
		}
	}

	public ChildInfo(string name, DomNodeType type)
		: this(name, type, isList: false)
	{
	}

	public ChildInfo(string name, DomNodeType type, bool isList)
		: base(name)
	{
		m_type = type;
		m_isList = isList;
	}

	public void AddRule(ChildRule rule)
	{
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		if (m_rules == null)
		{
			m_rules = new List<ChildRule>(1);
		}
		m_rules.Add(rule);
	}

	public virtual bool Validate(DomNode parent, DomNode child)
	{
		if (m_rules != null)
		{
			foreach (ChildRule rule in m_rules)
			{
				if (!rule.Validate(parent, child, this))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsEquivalent(ChildInfo other)
	{
		return other != null && base.Index == other.Index && base.DefiningType == other.DefiningType;
	}

	protected override NamedMetadata GetParent()
	{
		if (base.OwningType != null)
		{
			return base.OwningType.BaseType.GetChildInfo(base.Index);
		}
		return null;
	}
}

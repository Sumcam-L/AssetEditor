using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class AttributeInfo : FieldMetadata
{
	private readonly AttributeType m_type;

	private object m_defaultValue;

	private List<AttributeRule> m_rules;

	public bool IsIdAttribute
	{
		get
		{
			if (base.OwningType != null)
			{
				return Equivalent(base.OwningType.IdAttribute);
			}
			return false;
		}
	}

	public AttributeType Type => m_type;

	public object DefaultValue
	{
		get
		{
			if (m_defaultValue != null)
			{
				return m_defaultValue;
			}
			return m_type.GetDefault();
		}
		set
		{
			if (value != null && !m_type.ClrType.IsAssignableFrom(value.GetType()))
			{
				throw new InvalidOperationException("Incompatible value type");
			}
			m_defaultValue = value;
		}
	}

	public IEnumerable<AttributeRule> Rules
	{
		get
		{
			if (m_rules != null)
			{
				return m_rules;
			}
			return EmptyEnumerable<AttributeRule>.Instance;
		}
	}

	public AttributeInfo(string name, AttributeType type)
		: base(name)
	{
		m_type = type;
	}

	public void AddRule(AttributeRule rule)
	{
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		if (m_rules == null)
		{
			m_rules = new List<AttributeRule>();
		}
		m_rules.Add(rule);
	}

	public virtual bool Validate(object value)
	{
		if (!m_type.Validate(value, this))
		{
			return false;
		}
		if (m_rules != null)
		{
			foreach (AttributeRule rule in m_rules)
			{
				if (!rule.Validate(value, this))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override NamedMetadata GetParent()
	{
		if (base.OwningType != null)
		{
			return base.OwningType.BaseType.GetAttributeInfo(base.Index);
		}
		return null;
	}
}

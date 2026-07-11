using System;

namespace Sce.Atf.Dom;

public class StringEnumRule : AttributeRule
{
	private readonly string[] m_values;

	public StringEnumRule(string[] values)
	{
		if (values == null || values.Length == 0)
		{
			throw new ArgumentException("values must be an array with at least one element");
		}
		m_values = values;
	}

	public override bool Validate(object value, AttributeInfo info)
	{
		string[] values = m_values;
		foreach (string text in values)
		{
			if (text.Equals(value))
			{
				return true;
			}
		}
		return false;
	}
}

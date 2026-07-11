using System;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class QueryPropertyValueAsNumberInput : QueryNumericalInput
{
	private readonly bool m_isReplacePattern;

	private QueryPropertyValueAsNumberInput()
		: base(null, NumericalQuery.All)
	{
	}

	public QueryPropertyValueAsNumberInput(QueryNode parentNode, NumericalQuery numericalQueryOptions, bool isReplacePattern)
		: base(parentNode, numericalQueryOptions)
	{
		m_isReplacePattern = isReplacePattern;
	}

	public override void BuildPredicate(IQueryPredicate predicate)
	{
		if (!double.TryParse(base.TextInput1, out var result))
		{
			return;
		}
		DomNodePropertyPredicate domNodePropertyPredicate = (DomNodePropertyPredicate)predicate;
		if (domNodePropertyPredicate == null)
		{
			throw new ArgumentException("DomNode-specific query tree has been passed an unhandled type of predicate info");
		}
		ulong tag = base.SelectedItem.Tag;
		switch (tag)
		{
		case 0uL:
		case 1uL:
		case 2uL:
		case 3uL:
		case 4uL:
		case 5uL:
		case 6uL:
		case 7uL:
		case 8uL:
		case 9uL:
		case 10uL:
		case 11uL:
		case 12uL:
		case 13uL:
		case 14uL:
		case 15uL:
		case 16uL:
		{
			ulong num = tag - 1;
			if (num <= 3)
			{
				switch ((uint)num)
				{
				case 0u:
					domNodePropertyPredicate.AddNumberValueEqualsExpression(result, m_isReplacePattern);
					return;
				case 1u:
					domNodePropertyPredicate.AddNumberValueLesserExpression(result, m_isReplacePattern);
					return;
				case 3u:
					domNodePropertyPredicate.AddNumberValueLesserEqualExpression(result, m_isReplacePattern);
					return;
				case 2u:
					return;
				}
			}
			if (tag == 16)
			{
				domNodePropertyPredicate.AddNumberValueGreaterEqualExpression(result, m_isReplacePattern);
			}
			break;
		}
		case 32uL:
			domNodePropertyPredicate.AddNumberValueGreaterExpression(result, m_isReplacePattern);
			break;
		case 64uL:
		{
			if (double.TryParse(base.TextInput2, out var result2))
			{
				domNodePropertyPredicate.AddNumberValueBetweenExpression(result, result2, m_isReplacePattern);
			}
			break;
		}
		}
	}
}

using System;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class QueryStringPropertyInput : QueryStringInput
{
	private readonly bool m_isReplacePattern;

	private QueryStringPropertyInput()
		: base(null, StringQuery.All)
	{
		m_isReplacePattern = false;
	}

	public QueryStringPropertyInput(QueryNode parentNode, StringQuery stringQueryOptions, bool isReplacePattern)
		: base(parentNode, stringQueryOptions)
	{
		m_isReplacePattern = isReplacePattern;
	}

	protected void BuildStringPredicate(DomNodePropertyPredicate predicate, DomNodeQuery.PropertySearchTarget target)
	{
		switch (target)
		{
		case DomNodeQuery.PropertySearchTarget.Name:
			predicate.AddNameStringSearchExpression(base.TextInput, base.SelectedItem.Tag, m_isReplacePattern);
			break;
		case DomNodeQuery.PropertySearchTarget.Value:
			predicate.AddValueStringSearchExpression(base.TextInput, base.SelectedItem.Tag, m_isReplacePattern);
			break;
		default:
			throw new InvalidOperationException("Unhandled property search target type");
		}
	}
}

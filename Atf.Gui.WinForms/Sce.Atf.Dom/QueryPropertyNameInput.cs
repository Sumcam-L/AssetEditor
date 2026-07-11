using System;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class QueryPropertyNameInput : QueryStringPropertyInput
{
	private QueryPropertyNameInput(bool isReplacePattern)
		: base(null, StringQuery.All, isReplacePattern)
	{
	}

	public QueryPropertyNameInput(QueryNode parentNode, StringQuery stringQueryOptions, bool isReplacePattern)
		: base(parentNode, stringQueryOptions, isReplacePattern)
	{
	}

	public override void BuildPredicate(IQueryPredicate predicate)
	{
		DomNodePropertyPredicate domNodePropertyPredicate = (DomNodePropertyPredicate)predicate;
		if (domNodePropertyPredicate == null)
		{
			throw new ArgumentException("DomNode-specific query tree has been passed an unhandled type of predicate info");
		}
		BuildStringPredicate(domNodePropertyPredicate, DomNodeQuery.PropertySearchTarget.Name);
	}
}

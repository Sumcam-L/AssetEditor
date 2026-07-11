using System;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class QueryDomNodeName : QueryPropertyValueAsStringInput
{
	private QueryDomNodeName()
		: base(null, StringQuery.All, isReplacePattern: false)
	{
	}

	public QueryDomNodeName(QueryNode parentNode, StringQuery stringQueryOptions, bool isReplacePattern)
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
		domNodePropertyPredicate.AddPropertyNameExpression("Name");
		base.BuildPredicate((IQueryPredicate)domNodePropertyPredicate);
	}
}

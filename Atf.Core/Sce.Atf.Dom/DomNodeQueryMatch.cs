using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class DomNodeQueryMatch
{
	private readonly DomNode m_domNode;

	private readonly Dictionary<IQueryPredicate, IList<IQueryMatch>> m_predicateMatchResults;

	public DomNode DomNode => m_domNode;

	public Dictionary<IQueryPredicate, IList<IQueryMatch>> PredicateMatchResults => m_predicateMatchResults;

	private DomNodeQueryMatch()
	{
	}

	public DomNodeQueryMatch(DomNode domNode, Dictionary<IQueryPredicate, IList<IQueryMatch>> predicateMatchResults)
	{
		m_domNode = domNode;
		m_predicateMatchResults = predicateMatchResults;
	}
}

using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public static class QueryTreeDomNode
{
	public static QueryNode AddDomNodeNameQuery(this QueryNode parentNode, bool isReplacePattern)
	{
		return new QueryDomNodeName(parentNode, StringQuery.All, isReplacePattern);
	}

	public static QueryNode AddDomNodePropertyQuery(this QueryNode parentNode)
	{
		return new QueryDomNodeProperty(parentNode);
	}
}

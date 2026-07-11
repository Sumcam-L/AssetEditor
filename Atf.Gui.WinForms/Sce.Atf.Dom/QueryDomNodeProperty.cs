using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class QueryDomNodeProperty : QueryNode
{
	private QueryDomNodeProperty()
	{
	}

	public QueryDomNodeProperty(QueryNode parentNode)
	{
		parentNode.Add(this);
		this.AddLabel("whose name");
		new QueryPropertyNameInput(this, StringQuery.Matches, isReplacePattern: false);
		this.AddLabel("and whose");
		QueryOption parentNode2 = this.AddOption();
		new QueryPropertyValueAsStringInput(parentNode2.AddOptionItem("string value", 0uL), StringQuery.All, isReplacePattern: true);
		new QueryPropertyValueAsNumberInput(parentNode2.AddOptionItem("numerical value", 0uL), NumericalQuery.All, isReplacePattern: true);
	}
}

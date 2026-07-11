namespace Sce.Atf.Applications;

public class QueryStringInput : QueryOption
{
	private readonly QueryTextInput m_textInput;

	public string TextInput => m_textInput.InputText;

	private QueryStringInput()
	{
	}

	public QueryStringInput(QueryNode parentNode, StringQuery stringQueryOptions)
	{
		m_textInput = null;
		parentNode.Add(this);
		QueryNode queryNode = parentNode;
		while (queryNode.Parent != null)
		{
			queryNode = queryNode.Parent as QueryNode;
		}
		if (queryNode is QueryRoot queryRoot)
		{
			queryRoot.RegisterQueryOption(this);
		}
		if (stringQueryOptions != StringQuery.None)
		{
			if ((stringQueryOptions & StringQuery.RegularExpression) != StringQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("matches the regular expression", 16uL);
				m_textInput = parentNode2.AddStringSearchTextInput(m_textInput);
			}
			if ((stringQueryOptions & StringQuery.Contains) != StringQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("contains", 2uL);
				m_textInput = parentNode2.AddStringSearchTextInput(m_textInput);
			}
			if ((stringQueryOptions & StringQuery.Matches) != StringQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("is", 1uL);
				m_textInput = parentNode2.AddStringSearchTextInput(m_textInput);
			}
			if ((stringQueryOptions & StringQuery.BeginsWith) != StringQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("begins with", 4uL);
				m_textInput = parentNode2.AddStringSearchTextInput(m_textInput);
			}
			if ((stringQueryOptions & StringQuery.EndsWith) != StringQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("ends with", 8uL);
				m_textInput = parentNode2.AddStringSearchTextInput(m_textInput);
			}
		}
	}

	public override void BuildPredicate(IQueryPredicate predicate)
	{
	}
}

namespace Sce.Atf.Applications;

public class QueryNumericalInput : QueryOption
{
	private readonly QueryTextInput m_textInput1;

	private readonly QueryTextInput m_textInput2;

	public string TextInput1 => m_textInput1.InputText;

	public string TextInput2 => m_textInput2.InputText;

	private QueryNumericalInput()
	{
	}

	public QueryNumericalInput(QueryNode parentNode, NumericalQuery numericalQueryOptions)
	{
		m_textInput1 = null;
		m_textInput2 = null;
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
		if (numericalQueryOptions != NumericalQuery.None)
		{
			if ((numericalQueryOptions & NumericalQuery.Equals) != NumericalQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("equals", 1uL);
				m_textInput1 = parentNode2.AddNumericalSearchTextInput(m_textInput1);
			}
			if ((numericalQueryOptions & NumericalQuery.Lesser) != NumericalQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("is less than", 2uL);
				m_textInput1 = parentNode2.AddNumericalSearchTextInput(m_textInput1);
			}
			if ((numericalQueryOptions & NumericalQuery.LesserEqual) != NumericalQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("is lesser or equal to", 4uL);
				m_textInput1 = parentNode2.AddNumericalSearchTextInput(m_textInput1);
			}
			if ((numericalQueryOptions & NumericalQuery.GreaterEqual) != NumericalQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("is greater or equal to", 16uL);
				m_textInput1 = parentNode2.AddNumericalSearchTextInput(m_textInput1);
			}
			if ((numericalQueryOptions & NumericalQuery.Greater) != NumericalQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("is greater than", 32uL);
				m_textInput1 = parentNode2.AddNumericalSearchTextInput(m_textInput1);
			}
			if ((numericalQueryOptions & NumericalQuery.Between) != NumericalQuery.None)
			{
				QueryOptionItem parentNode2 = this.AddOptionItem("is between", 64uL);
				m_textInput1 = parentNode2.AddNumericalSearchTextInput(m_textInput1);
				parentNode2.AddLabel("and");
				m_textInput2 = parentNode2.AddNumericalSearchTextInput(m_textInput2);
			}
		}
	}

	public override void BuildPredicate(IQueryPredicate predicate)
	{
	}
}

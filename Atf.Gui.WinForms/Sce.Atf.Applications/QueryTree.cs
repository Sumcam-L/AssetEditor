namespace Sce.Atf.Applications;

public static class QueryTree
{
	public static QueryNode Add(this QueryNode parentNode, QueryNode childNode)
	{
		parentNode.Children.Add(childNode);
		return childNode;
	}

	public static QueryLabel AddLabel(this QueryNode parentNode, string text)
	{
		return parentNode.Add(new QueryLabel(text)) as QueryLabel;
	}

	public static QuerySeparator AddSeparator(this QueryNode parentNode)
	{
		return parentNode.Add(new QuerySeparator()) as QuerySeparator;
	}

	public static QueryButton AddButton(this QueryNode parentNode, string text)
	{
		return parentNode.Add(new QueryButton(text)) as QueryButton;
	}

	public static QueryOption AddOption(this QueryNode parentNode)
	{
		QueryOption queryOption = parentNode.Add(new QueryOption()) as QueryOption;
		if (queryOption.Root is QueryRoot queryRoot)
		{
			queryRoot.RegisterQueryOption(queryOption);
		}
		return queryOption;
	}

	public static QueryOptionItem AddOptionItem(this QueryOption parentNode, string optionItemText, ulong tag)
	{
		QueryOptionItem queryOptionItem = null;
		if (parentNode != null)
		{
			queryOptionItem = parentNode.Add(new QueryOptionItem(optionItemText, tag)) as QueryOptionItem;
			parentNode.RegisterOptionItem(queryOptionItem);
		}
		return queryOptionItem;
	}

	public static QueryTextInput AddTextInput(this QueryNode parentNode, QueryTextInput textInput, bool isNumericalText)
	{
		bool flag = textInput != null;
		return parentNode.Add(flag ? textInput : new QueryTextInput(isNumericalText)) as QueryTextInput;
	}

	public static QueryTextInput AddStringSearchTextInput(this QueryNode parentNode, QueryTextInput textInput)
	{
		QueryTextInput queryTextInput = parentNode.AddTextInput(textInput, isNumericalText: false);
		if (queryTextInput != null && queryTextInput != textInput && queryTextInput.Root is QueryRoot queryRoot)
		{
			queryRoot.RegisterSearchQueryTextInput(queryTextInput);
		}
		return queryTextInput;
	}

	public static QueryTextInput AddNumericalSearchTextInput(this QueryNode parentNode, QueryTextInput textInput)
	{
		QueryTextInput queryTextInput = parentNode.AddTextInput(textInput, isNumericalText: true);
		if (queryTextInput != null && queryTextInput != textInput && queryTextInput.Root is QueryRoot queryRoot)
		{
			queryRoot.RegisterSearchQueryTextInput(queryTextInput);
		}
		return queryTextInput;
	}

	public static QueryTextInput AddReplaceTextInput(this QueryNode parentNode, QueryTextInput textInput, bool isNumericalText)
	{
		QueryTextInput queryTextInput = parentNode.AddTextInput(textInput, isNumericalText);
		if (queryTextInput != null && queryTextInput != textInput && queryTextInput.Root is QueryRoot queryRoot)
		{
			queryRoot.RegisterReplaceQueryTextInput(queryTextInput);
		}
		return queryTextInput;
	}

	public static QueryOption AddNumericalQuery(this QueryNode parentNode, NumericalQuery numericalQueryOptions)
	{
		return new QueryNumericalInput(parentNode, numericalQueryOptions);
	}
}

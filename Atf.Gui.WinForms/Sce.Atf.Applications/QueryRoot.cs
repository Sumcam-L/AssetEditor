using System;

namespace Sce.Atf.Applications;

public class QueryRoot : QueryNode
{
	internal bool QueryDirty { get; set; }

	public event EventHandler SearchTextEntered;

	public event EventHandler ReplaceTextEntered;

	public event EventHandler OptionChanged;

	public void RegisterSearchQueryTextInput(QueryTextInput queryTextInput)
	{
		queryTextInput.TextEntered += childNode_SearchTextEntered;
		queryTextInput.TextChanged += childNode_SearchTextChanged;
	}

	public void RegisterSearchButtonPress(QueryButton queryButton)
	{
		queryButton.Clicked += childNode_SearchTextEntered;
	}

	public void RegisterReplaceButtonPress(QueryButton queryButton)
	{
		queryButton.Clicked += childNode_ReplaceTextEntered;
	}

	public void RegisterReplaceQueryTextInput(QueryTextInput queryTextInput)
	{
		queryTextInput.TextEntered += childNode_ReplaceTextEntered;
	}

	public void RegisterQueryOption(QueryOption queryOption)
	{
		queryOption.OptionChanged += childNode_OptionChanged;
	}

	private void childNode_SearchTextEntered(object sender, EventArgs args)
	{
		this.SearchTextEntered.Raise(sender, EventArgs.Empty);
		QueryDirty = false;
	}

	private void childNode_SearchTextChanged(object sender, EventArgs e)
	{
		QueryDirty = true;
	}

	private void childNode_ReplaceTextEntered(object sender, EventArgs args)
	{
		if (QueryDirty)
		{
			this.SearchTextEntered.Raise(sender, EventArgs.Empty);
			QueryDirty = false;
		}
		this.ReplaceTextEntered.Raise(sender, EventArgs.Empty);
	}

	private void childNode_OptionChanged(object sender, EventArgs args)
	{
		this.OptionChanged.Raise(sender, EventArgs.Empty);
	}

	public virtual IQueryPredicate GetPredicate()
	{
		return null;
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public abstract class SearchTextBox : TextBox, ISearchUI, ISearchableContextUI
{
	private IQueryableContext m_queryableContext;

	public Control Control => this;

	protected IQueryableContext QueryableContext
	{
		get
		{
			return m_queryableContext;
		}
		private set
		{
			m_queryableContext = value;
		}
	}

	public abstract event EventHandler UIChanged;

	public void Bind(IQueryableContext queryableContext)
	{
		QueryableContext = queryableContext;
		base.Enabled = queryableContext != null;
	}

	public abstract IQueryPredicate GetPredicate();

	public IEnumerable<object> DoSearch()
	{
		if (QueryableContext == null)
		{
			return null;
		}
		return QueryableContext.Query(GetPredicate());
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public abstract class SearchToolStrip : ToolStrip, ISearchUI, ISearchableContextUI
{
	public Control Control => this;

	protected IQueryableContext QueryableContext { get; private set; }

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

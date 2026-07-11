using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class QueryNode : Tree<QueryNode>
{
	public QueryNode Root
	{
		get
		{
			QueryNode queryNode = this;
			while (queryNode.Parent != null)
			{
				queryNode = queryNode.Parent as QueryNode;
			}
			return queryNode;
		}
	}

	protected QueryNode FirstChild => (base.Children.Count > 0) ? base.Children[0].Value : null;

	public virtual void GetToolStripItems(List<ToolStripItem> items)
	{
		foreach (QueryNode child in base.Children)
		{
			child.GetToolStripItems(items);
		}
	}

	public virtual ToolStripItem GetToolStripItem()
	{
		ToolStripItem toolStripItem = null;
		return FirstChild.GetToolStripItem();
	}

	public virtual void BuildPredicate(IQueryPredicate predicate)
	{
		foreach (QueryNode child in base.Children)
		{
			child.BuildPredicate(predicate);
		}
	}
}

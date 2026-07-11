using System;
using System.Windows.Forms;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

public abstract class SearchResultsListView : ListView, IResultsUI, ISearchableContextUI
{
	private IQueryableResultContext m_queryResultContext;

	public Control Control => this;

	protected IQueryableResultContext QueryResultContext
	{
		get
		{
			if (m_queryResultContext == null)
			{
				throw new InvalidOperationException("Search Results UI isn't bound to a data set.");
			}
			return m_queryResultContext;
		}
	}

	public abstract event EventHandler UIChanged;

	public SearchResultsListView(IContextRegistry contextRegistry)
	{
		base.View = View.Details;
		base.LabelEdit = false;
		base.AllowColumnReorder = false;
		base.CheckBoxes = false;
		base.FullRowSelect = true;
		base.MultiSelect = false;
		base.GridLines = true;
		base.Sorting = SortOrder.Ascending;
	}

	public SearchResultsListView(IQueryableResultContext queryResultContext, IContextRegistry contextRegistry)
		: this(contextRegistry)
	{
		Bind(queryResultContext);
	}

	public void Bind(IQueryableResultContext queryResultContext)
	{
		ClearResults();
		if (m_queryResultContext != null)
		{
			m_queryResultContext.ResultsChanged -= queryResultContext_ResultsChanged;
		}
		m_queryResultContext = queryResultContext;
		if (m_queryResultContext != null)
		{
			m_queryResultContext.ResultsChanged += queryResultContext_ResultsChanged;
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (m_queryResultContext == null)
		{
			return;
		}
		ISelectionContext selectionContext = m_queryResultContext.As<ISelectionContext>();
		if (selectionContext == null)
		{
			return;
		}
		ListViewHitTestInfo listViewHitTestInfo = HitTest(e.Location);
		if (listViewHitTestInfo != null && listViewHitTestInfo.Item != null)
		{
			object tag = listViewHitTestInfo.Item.Tag;
			selectionContext.Set(tag);
			ISubSelectionContext subSelectionContext = m_queryResultContext.As<ISubSelectionContext>();
			if (subSelectionContext == null)
			{
				return;
			}
			object obj = null;
			if (listViewHitTestInfo.SubItem != null && listViewHitTestInfo.SubItem.Tag != null)
			{
				obj = listViewHitTestInfo.SubItem.Tag;
			}
			else
			{
				foreach (object subItem in listViewHitTestInfo.Item.SubItems)
				{
					if (subItem is ListViewItem.ListViewSubItem { Tag: not null } listViewSubItem)
					{
						obj = listViewSubItem.Tag;
						break;
					}
				}
			}
			if (obj != null)
			{
				subSelectionContext.Set(obj);
			}
			else
			{
				subSelectionContext.Clear();
			}
		}
		else
		{
			selectionContext.Clear();
		}
	}

	private void queryResultContext_ResultsChanged(object sender, EventArgs e)
	{
		UpdateResults();
	}

	protected abstract void ClearResults();

	protected abstract void UpdateResults();
}

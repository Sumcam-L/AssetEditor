using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class MultipleHistoryContext : Observer
{
	private IDocument m_document;

	private bool m_synchronizing;

	private IEnumerable<IHistoryContext> HistoryContexts
	{
		get
		{
			foreach (DomNode node in DomNode.Subtree)
			{
				foreach (IHistoryContext item in node.AsAll<IHistoryContext>())
				{
					yield return item;
				}
			}
		}
	}

	protected override void OnNodeSet()
	{
		m_document = base.DomNode.Cast<IDocument>();
		m_document.DirtyChanged += document_DirtyChanged;
		base.OnNodeSet();
	}

	protected override void AddNode(DomNode node)
	{
		foreach (IHistoryContext item in node.AsAll<IHistoryContext>())
		{
			item.DirtyChanged += History_DirtyChanged;
		}
	}

	protected override void RemoveNode(DomNode node)
	{
		foreach (IHistoryContext item in node.AsAll<IHistoryContext>())
		{
			item.DirtyChanged -= History_DirtyChanged;
		}
	}

	private void History_DirtyChanged(object sender, EventArgs e)
	{
		if (m_synchronizing)
		{
			return;
		}
		try
		{
			m_synchronizing = true;
			bool dirty = false;
			foreach (IHistoryContext historyContext in HistoryContexts)
			{
				if (historyContext.Dirty)
				{
					dirty = true;
					break;
				}
			}
			m_document.Dirty = dirty;
		}
		finally
		{
			m_synchronizing = false;
		}
	}

	private void document_DirtyChanged(object sender, EventArgs e)
	{
		if (m_synchronizing)
		{
			return;
		}
		try
		{
			m_synchronizing = true;
			IDocument document = (IDocument)sender;
			bool dirty = document.Dirty;
			foreach (IHistoryContext historyContext in HistoryContexts)
			{
				historyContext.Dirty = dirty;
			}
		}
		finally
		{
			m_synchronizing = false;
		}
	}
}

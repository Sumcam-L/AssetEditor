using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class GlobalHistoryContext : Observer
{
	private HistoryContext m_historyContext;

	private HashSet<HistoryContext> m_childHistoryContexts;

	protected override void OnNodeSet()
	{
		m_historyContext = base.DomNode.Cast<HistoryContext>();
		m_childHistoryContexts = new HashSet<HistoryContext>();
		base.OnNodeSet();
	}

	protected override void AddNode(DomNode node)
	{
		foreach (HistoryContext item in node.AsAll<HistoryContext>())
		{
			if (item != m_historyContext)
			{
				m_childHistoryContexts.Add(item);
				item.History = m_historyContext.History;
			}
		}
	}

	protected override void RemoveNode(DomNode node)
	{
		foreach (HistoryContext item in node.AsAll<HistoryContext>())
		{
			if (item != m_historyContext)
			{
				m_childHistoryContexts.Remove(item);
				item.History = new CommandHistory();
			}
		}
	}

	internal void SynchronizeUndoRedoStatus(bool newStatus)
	{
		m_historyContext.UndoingOrRedoing = newStatus;
		foreach (HistoryContext childHistoryContext in m_childHistoryContexts)
		{
			childHistoryContext.UndoingOrRedoing = newStatus;
		}
	}
}

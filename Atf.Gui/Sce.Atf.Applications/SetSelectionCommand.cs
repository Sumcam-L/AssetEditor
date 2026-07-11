using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Applications;

public class SetSelectionCommand : Command
{
	private readonly ISelectionContext m_selectionContext;

	private readonly IEnumerable<object> m_previous;

	private readonly IEnumerable<object> m_next;

	public SetSelectionCommand(ISelectionContext selectionContext, IEnumerable<object> nextSelection)
		: this(selectionContext, selectionContext.Selection, nextSelection)
	{
	}

	public SetSelectionCommand(ISelectionContext selectionContext, IEnumerable<object> previous, IEnumerable<object> next)
		: base("Set Selection".Localize())
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		m_selectionContext = selectionContext;
		m_previous = Snapshot(previous);
		if (previous.SequenceEqual(next))
		{
			m_next = m_previous;
		}
		else
		{
			m_next = Snapshot(next);
		}
	}

	public override void Do()
	{
		m_selectionContext.SetRange(m_next);
	}

	public override void Undo()
	{
		m_selectionContext.SetRange(m_previous);
	}

	private IEnumerable<object> Snapshot(IEnumerable<object> enumerable)
	{
		if (enumerable != null)
		{
			return enumerable.ToArray();
		}
		return EmptyEnumerable<object>.Instance;
	}
}

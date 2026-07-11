using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class EditingContext : HistoryContext, ISelectionContext
{
	private readonly AdaptableSelection<object> m_selection;

	private IEnumerable<object> m_prevSelection;

	public bool FastEditing { get; set; }

	public Selection<object> Selection => m_selection;

	public bool RecordSelectionChanges { get; set; }

	IEnumerable<object> ISelectionContext.Selection
	{
		get
		{
			return m_selection;
		}
		set
		{
			m_selection.SetRange(value);
		}
	}

	public object LastSelected => m_selection.LastSelected;

	public int SelectionCount => m_selection.Count;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public EditingContext()
	{
		m_selection = new AdaptableSelection<object>();
		m_selection.Changing += selection_Changing;
		m_selection.Changed += selection_Changed;
		FastEditing = false;
	}

	public IEnumerable<T> GetSelection<T>() where T : class
	{
		return m_selection.AsIEnumerable<T>();
	}

	public T GetLastSelected<T>() where T : class
	{
		return m_selection.GetLastSelected<T>();
	}

	public virtual bool SelectionContains(object item)
	{
		return m_selection.Contains(item);
	}

	private void selection_Changing(object sender, EventArgs e)
	{
		this.SelectionChanging.Raise(this, e);
		if (RecordSelectionChanges && base.Recording && !InTransaction && !base.UndoingOrRedoing)
		{
			m_prevSelection = m_selection.ToArray();
		}
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		if (RecordSelectionChanges && base.Recording && !InTransaction && !base.UndoingOrRedoing)
		{
			base.History.Add(new SetSelectionCommand(this, m_prevSelection, m_selection.ToArray()));
		}
		this.SelectionChanged.Raise(this, e);
	}
}

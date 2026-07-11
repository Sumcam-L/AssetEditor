using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class SelectionContext : DomNodeAdapter, ISelectionContext
{
	private readonly AdaptableSelection<object> m_selection;

	public AdaptableSelection<object> Selection => m_selection;

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

	int ISelectionContext.SelectionCount => m_selection.Count;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public SelectionContext()
	{
		m_selection = new AdaptableSelection<object>();
		m_selection.Changing += selection_Changing;
		m_selection.Changed += selection_Changed;
	}

	public IEnumerable<T> GetSelection<T>() where T : class
	{
		return m_selection.AsIEnumerable<T>();
	}

	public T GetLastSelected<T>() where T : class
	{
		return m_selection.GetLastSelected<T>();
	}

	public bool SelectionContains(object item)
	{
		return m_selection.Contains(item);
	}

	private void selection_Changing(object sender, EventArgs e)
	{
		this.SelectionChanging.Raise(this, e);
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		this.SelectionChanged.Raise(this, e);
	}
}

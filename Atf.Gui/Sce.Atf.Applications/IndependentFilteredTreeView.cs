using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

public class IndependentFilteredTreeView : BasicFilteredTreeView, ISelectionContext
{
	private readonly AdaptableSelection<object> m_selection;

	public override object Root => m_treeView.Root;

	public IEnumerable<object> Selection
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

	public IndependentFilteredTreeView(ITreeView treeView, Predicate<object> filterFunc)
		: base(treeView, filterFunc)
	{
		m_selection = new AdaptableSelection<object>();
		m_selection.Changing += delegate(object s, EventArgs e)
		{
			this.SelectionChanging.Raise(this, e);
		};
		m_selection.Changed += delegate(object s, EventArgs e)
		{
			this.SelectionChanged.Raise(this, e);
		};
	}

	public override object GetAdapter(Type type)
	{
		if (typeof(IndependentFilteredTreeView).IsAssignableFrom(type))
		{
			return this;
		}
		if (type == typeof(IItemView))
		{
			return base.InnerTreeView;
		}
		return null;
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
}

using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Controls;

public class TreeViewWithSelection : ITreeView, IItemView, ISelectionContext
{
	private readonly IItemView m_itemView;

	private readonly ITreeView m_treeView;

	private readonly AdaptableSelection<object> m_selection;

	public object Root => m_treeView.Root;

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

	public TreeViewWithSelection(ITreeView root)
	{
		m_treeView = root;
		m_itemView = root as IItemView;
		m_selection = new AdaptableSelection<object>();
		m_selection.Changed += delegate(object s, EventArgs e)
		{
			this.SelectionChanged.Raise(this, e);
		};
		m_selection.Changing += delegate(object s, EventArgs e)
		{
			this.SelectionChanging.Raise(this, e);
		};
	}

	public virtual IEnumerable<object> GetChildren(object parent)
	{
		return m_treeView.GetChildren(parent);
	}

	public void GetInfo(object item, ItemInfo info)
	{
		if (m_itemView != null)
		{
			m_itemView.GetInfo(item, info);
		}
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

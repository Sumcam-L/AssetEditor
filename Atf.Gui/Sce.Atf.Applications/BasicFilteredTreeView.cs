using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

public class BasicFilteredTreeView : ITreeView, IAdaptable
{
	protected readonly ITreeView m_treeView;

	private readonly Predicate<object> m_filterFunc;

	public ITreeView InnerTreeView => m_treeView;

	public virtual object Root => m_treeView.Root;

	public BasicFilteredTreeView(ITreeView treeView, Predicate<object> filterFunc)
	{
		m_treeView = treeView;
		m_filterFunc = filterFunc;
	}

	public static bool Equals(ITreeView first, ITreeView second)
	{
		BasicFilteredTreeView basicFilteredTreeView = first.As<BasicFilteredTreeView>();
		if (basicFilteredTreeView != null)
		{
			first = basicFilteredTreeView.m_treeView;
		}
		BasicFilteredTreeView basicFilteredTreeView2 = second.As<BasicFilteredTreeView>();
		if (basicFilteredTreeView2 != null)
		{
			second = basicFilteredTreeView2.m_treeView;
		}
		return first == second;
	}

	public virtual IEnumerable<object> GetChildren(object parent)
	{
		return from child in m_treeView.GetChildren(parent)
			where MatchOrDescendantMatch(child)
			select child;
	}

	public virtual object GetAdapter(Type type)
	{
		if (typeof(BasicFilteredTreeView).IsAssignableFrom(type))
		{
			return this;
		}
		return m_treeView;
	}

	private bool MatchOrDescendantMatch(object item)
	{
		return m_filterFunc(item) || m_treeView.GetChildren(item).Any((object child) => MatchOrDescendantMatch(child));
	}
}

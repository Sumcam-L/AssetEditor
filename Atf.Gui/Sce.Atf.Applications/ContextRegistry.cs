using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

[Export(typeof(IContextRegistry))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ContextRegistry : IContextRegistry
{
	private readonly AdaptableActiveCollection<object> m_contexts;

	public AdaptableActiveCollection<object> Contexts => m_contexts;

	public object ActiveContext
	{
		get
		{
			return m_contexts.ActiveItem;
		}
		set
		{
			m_contexts.ActiveItem = value;
		}
	}

	IEnumerable<object> IContextRegistry.Contexts => m_contexts;

	public event EventHandler ActiveContextChanging;

	public event EventHandler ActiveContextChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ContextAdded;

	public event EventHandler<ItemRemovedEventArgs<object>> ContextRemoved;

	public ContextRegistry()
	{
		m_contexts = new AdaptableActiveCollection<object>();
		m_contexts.ActiveItemChanged += contexts_ActiveItemChanged;
		m_contexts.ActiveItemChanging += contexts_ActiveItemChanging;
		m_contexts.ItemAdded += contexts_ItemAdded;
		m_contexts.ItemRemoved += contexts_ItemRemoved;
	}

	public T GetActiveContext<T>() where T : class
	{
		return m_contexts.ActiveItem.As<T>();
	}

	public T GetMostRecentContext<T>() where T : class
	{
		return m_contexts.GetActiveItem<T>();
	}

	public bool RemoveContext(object context)
	{
		return m_contexts.Remove(context);
	}

	private void contexts_ActiveItemChanging(object sender, EventArgs e)
	{
		this.ActiveContextChanging.Raise(this, EventArgs.Empty);
	}

	private void contexts_ActiveItemChanged(object sender, EventArgs e)
	{
		this.ActiveContextChanged.Raise(this, EventArgs.Empty);
	}

	private void contexts_ItemAdded(object sender, ItemInsertedEventArgs<object> e)
	{
		this.ContextAdded.Raise(this, e);
	}

	private void contexts_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		this.ContextRemoved.Raise(this, e);
	}
}

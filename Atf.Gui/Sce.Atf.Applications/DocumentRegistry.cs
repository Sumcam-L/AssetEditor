using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

[Export(typeof(IDocumentRegistry))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DocumentRegistry : IDocumentRegistry
{
	private readonly AdaptableActiveCollection<IDocument> m_documents;

	public IDocument ActiveDocument
	{
		get { return m_documents.ActiveItem; }
		set { m_documents.ActiveItem = value; }
	}

	public IEnumerable<IDocument> Documents => m_documents;

	public event EventHandler<UriChangedEventArgs> UriChanged;
	public event EventHandler ActiveDocumentChanging;
	public event EventHandler ActiveDocumentChanged;
	public event EventHandler<ItemInsertedEventArgs<IDocument>> DocumentAdded;
	public event EventHandler<ItemRemovedEventArgs<IDocument>> DocumentRemoved;

	public DocumentRegistry()
	{
		m_documents = new AdaptableActiveCollection<IDocument>();
		m_documents.ActiveItemChanging += Documents_ActiveItemChanging;
		m_documents.ActiveItemChanged += Documents_ActiveItemChanged;
		m_documents.ItemAdded += Documents_ItemAdded;
		m_documents.ItemRemoved += Documents_ItemRemoved;
	}

	public T GetActiveDocument<T>() where T : class
	{
		return m_documents.ActiveItem.As<T>();
	}

	public T GetMostRecentDocument<T>() where T : class
	{
		foreach (IDocument item in m_documents.MostRecentOrder)
		{
			T val = item.As<T>();
			if (val != null)
				return val;
		}
		return null;
	}

	public void Remove(IDocument document)
	{
		m_documents.Remove(document);
	}

	private void Documents_ActiveItemChanging(object sender, EventArgs e)
	{
		var sw = Stopwatch.StartNew();
		this.ActiveDocumentChanging.Raise(this, e);
		sw.Stop();
		if (sw.ElapsedMilliseconds > 1)
			PaintTimingLog.Write("ActiveDocumentChanging event: {0}ms", sw.ElapsedMilliseconds);
	}

	private void Documents_ActiveItemChanged(object sender, EventArgs e)
	{
		var swTotal = Stopwatch.StartNew();
		Delegate[] inv = this.ActiveDocumentChanged?.GetInvocationList();
		if (inv != null)
		{
			foreach (Delegate d in inv)
			{
				var sw = Stopwatch.StartNew();
				d.DynamicInvoke(this, e);
				sw.Stop();
				if (sw.ElapsedMilliseconds > 1)
					PaintTimingLog.Write("ActiveDocumentChanged subscriber '{0}': {1}ms", d.Method?.DeclaringType?.Name ?? "?", sw.ElapsedMilliseconds);
			}
		}
		swTotal.Stop();
		if (swTotal.ElapsedMilliseconds > 1)
			PaintTimingLog.Write("ActiveDocumentChanged total: {0}ms", swTotal.ElapsedMilliseconds);
	}

	private void Documents_ItemAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		this.DocumentAdded.Raise(this, e);
		e.Item.UriChanged -= Document_UriChanged;
		e.Item.UriChanged += Document_UriChanged;
	}

	private void Documents_ItemRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		e.Item.UriChanged -= Document_UriChanged;
		this.DocumentRemoved.Raise(this, e);
	}

	private void Document_UriChanged(object sender, UriChangedEventArgs e)
	{
		this.UriChanged.Raise(this, e);
	}
}

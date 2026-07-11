using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IDocumentRegistry
{
	IDocument ActiveDocument { get; set; }

	IEnumerable<IDocument> Documents { get; }

	event EventHandler<UriChangedEventArgs> UriChanged;

	event EventHandler ActiveDocumentChanging;

	event EventHandler ActiveDocumentChanged;

	event EventHandler<ItemInsertedEventArgs<IDocument>> DocumentAdded;

	event EventHandler<ItemRemovedEventArgs<IDocument>> DocumentRemoved;

	T GetActiveDocument<T>() where T : class;

	T GetMostRecentDocument<T>() where T : class;

	void Remove(IDocument document);
}

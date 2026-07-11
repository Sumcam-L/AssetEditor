using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface IDocumentService
{
	event EventHandler<DocumentEventArgs> DocumentOpened;

	event EventHandler<DocumentEventArgs> DocumentSaving;

	event EventHandler<DocumentEventArgs> DocumentSaved;

	event EventHandler<DocumentEventArgs> DocumentClosed;

	event EventHandler<DocumentClosingEventArgs> DocumentClosing;

	IDocument OpenNewDocument(IDocumentClient client);

	IDocument OpenExistingDocument(IDocumentClient client, Uri uri);

	bool Save(IDocument document);

	bool SaveAs(IDocument document);

	bool SaveAll(bool cancelOnFail);

	bool Close(IDocument document);

	bool CloseAll(IDocument masterDocument);

	bool IsUntitled(IDocument document);

	void AddLockedDocumentPaths(IEnumerable<string> paths, string reason);

	void RemoveLockedDocumentPaths(IEnumerable<string> paths);

	void ClearLockedDocumentPaths();
}

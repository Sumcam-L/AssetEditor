using System;

namespace Sce.Atf.Applications;

public interface IDocumentClient
{
	bool AskWhenClosingDirtyDocument { get; }

	DocumentClientInfo Info { get; }

	bool CanOpen(Uri uri);

	IDocument Open(Uri uri);

	void Reload(IDocument document);

	void Show(IDocument document);

	bool Save(IDocument document, Uri uri);

	void Close(IDocument document);
}

using System;

namespace Sce.Atf.Applications;

public class DocumentEventArgs : EventArgs
{
	public readonly IDocument Document;

	public readonly DocumentEventType Kind;

	public DocumentEventArgs(IDocument document)
	{
		Document = document;
		Kind = DocumentEventType.UnKnown;
	}

	public DocumentEventArgs(IDocument document, DocumentEventType kind)
	{
		Document = document;
		Kind = kind;
	}
}

namespace Sce.Atf.Applications;

public class DocumentClosingEventArgs : DocumentEventArgs
{
	public bool Cancel { get; set; }

	public DocumentClosingEventArgs(IDocument document)
		: base(document)
	{
	}
}

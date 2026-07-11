using System;

namespace Firaxis.ATF;

public class DocumentImportedEventArgs : EventArgs
{
	public readonly IImportableDocument Document;

	public readonly bool Successful;

	public DocumentImportedEventArgs(IImportableDocument doc, bool success)
	{
		Document = doc;
		Successful = success;
	}
}

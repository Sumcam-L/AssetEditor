using System;

namespace Firaxis.ATF;

public class DocumentImportingEventArgs : EventArgs
{
	public readonly IImportableDocument Document;

	public DocumentImportingEventArgs(IImportableDocument doc)
	{
		Document = doc;
	}
}

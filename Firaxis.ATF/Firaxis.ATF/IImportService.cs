using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IImportService
{
	event EventHandler<DocumentImportedEventArgs> DocumentImported;

	event EventHandler<DocumentImportingEventArgs> DocumentImporting;

	event EventHandler ImportCompleted;

	void Import(IImportableDocument document, bool force = false);

	void Import(IEnumerable<IImportableDocument> documents, bool force = false);
}

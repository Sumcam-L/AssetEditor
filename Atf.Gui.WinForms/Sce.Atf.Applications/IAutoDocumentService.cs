using System;

namespace Sce.Atf.Applications;

public interface IAutoDocumentService
{
	bool AutoLoadDocuments { get; set; }

	bool AutoNewDocument { get; set; }

	string AutoDocuments { get; set; }

	event EventHandler AutoDocumentsOpening;

	event EventHandler AutoDocumentsOpened;
}

using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IPreviewerDocumentService : ISequencedProjectChangeWatcher
{
	void ApplyWorkspaceChanges(IEnumerable<Uri> changedFiles);

	IPreviewWindow GetWindowForDocument(IDocument doc);
}

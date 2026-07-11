using System;
using System.Collections.Generic;
using Sce.Atf;

namespace Firaxis.ATF;

public interface ITunerQueueService
{
	void AddDocumentToQueue(IDocument document);

	void AddFileToQueue(Uri uri);

	void AddFilesToQueue(IEnumerable<Uri> uris);
}

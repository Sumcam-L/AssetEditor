using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public static class DocumentServices
{
	public static IDocument OpenExistingDocument(this IDocumentService documentService, IEnumerable<IDocumentClient> documentClients, Uri uri)
	{
		foreach (IDocumentClient documentClient in documentClients)
		{
			if (documentClient.CanOpen(uri))
			{
				return documentService.OpenExistingDocument(documentClient, uri);
			}
		}
		return null;
	}
}

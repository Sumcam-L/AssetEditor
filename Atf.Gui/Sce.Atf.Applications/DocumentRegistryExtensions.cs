using System;
using System.Linq;

namespace Sce.Atf.Applications;

public static class DocumentRegistryExtensions
{
	public static IDocument GetDocument(this IDocumentRegistry registry, Uri uri)
	{
		return registry.Documents.FirstOrDefault((IDocument doc) => doc.Uri == uri);
	}

	public static bool IsOpen(this IDocumentRegistry registry, Uri uri)
	{
		return registry.GetDocument(uri) != null;
	}
}

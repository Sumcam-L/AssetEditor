using System;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IDocumentRegistryMediator : IDocumentRegistryProvider, IShadowDocumentRegistryProvider
{
	bool ShadowMode { get; set; }

	IDocument GetDocument(Uri uri);
}

using Sce.Atf.Applications;

namespace Firaxis.ATF;

public interface IShadowDocumentRegistryProvider
{
	IDocumentRegistry DocumentRegistry { get; }
}

using Sce.Atf.Applications;

namespace Firaxis.ATF;

public interface IDocumentRegistryProvider
{
	IDocumentRegistry DocumentRegistry { get; }
}

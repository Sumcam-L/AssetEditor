using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

public interface ISelectionPathProvider
{
	SelectionPathProviderInfo Info { get; }

	AdaptablePath<object> GetSelectionPath(object item);

	bool RemoveSelectionPath(object item);

	void UpdateSelectionPath(object item, AdaptablePath<object> path);

	AdaptablePath<object> IncludedPath(object item);
}

namespace Firaxis.CivTech;

public interface IWorkspaceDependencyRegistryService
{
	void StartProjectChange();

	void HandleProjectChange();

	void FinishProjectChange();
}

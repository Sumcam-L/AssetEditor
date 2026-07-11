using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public interface IPreviewerEntityLoadingService : ISequencedProjectChangeWatcher
{
	IInstanceSet InstanceSet { get; }

	IInstanceEntity LoadEntity(string name, InstanceType type);

	void UnloadEntity(IInstanceEntity entity);
}

using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public interface ITriggerInstancingContext : IInstancingContext
{
	void InsertAtTime(float time, object dataObject);
}

using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IPreviewable
{
	IInstanceEntity Entity { get; }
}

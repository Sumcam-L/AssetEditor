using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.AssetEditing;

public interface IAnimatableEntityAdapter : IInstanceEntityAdapter, INamedAdapter
{
	AnimationBindingSetAdapter AnimationBindingSet { get; }

	IAnimatable AnimationData { get; }

	string DSG { get; set; }

	IDSGInstance DSGInst { get; }

	IAssetPreviewer PreviewerService { get; }
}

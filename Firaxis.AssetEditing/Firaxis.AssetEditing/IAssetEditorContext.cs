using Firaxis.ATF;
using Sce.Atf;

namespace Firaxis.AssetEditing;

public interface IAssetEditorContext : IEntityEditorContext, IObservableContext
{
	IPropertyEditingListContext AnimationSetContext { get; }

	IPropertyEditingListContext AttachmentsContext { get; }

	IPropertyEditingListContext BehaviorSetContext { get; }

	IModelInstanceStateContext GeometrySetContext { get; }

	IPropertyEditingListContext ParticleEffectsContext { get; }

	IPropertyEditingListContext SplineSetContext { get; }

	bool HasSplines { get; }

	bool HasAnimations { get; }

	bool HasGeometries { get; }

	bool HasParticleEffects { get; }

	bool HasBehaviors { get; }
}

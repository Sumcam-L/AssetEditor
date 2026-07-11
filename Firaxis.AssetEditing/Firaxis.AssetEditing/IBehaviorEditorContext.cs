using Firaxis.ATF;
using Sce.Atf;

namespace Firaxis.AssetEditing;

public interface IBehaviorEditorContext : IEntityEditorContext, IObservableContext
{
	IPropertyEditingListContext AnimationSetContext { get; }

	IPropertyEditingListContext AttachmentsContext { get; }

	IPropertyEditingListContext BehaviorSetContext { get; }

	IPropertyEditingListContext GeometryReferenceSetContext { get; }

	bool HasAnimations { get; }

	bool HasGeometryReferences { get; }

	bool HasBehaviors { get; }
}
